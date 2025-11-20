using LayoutSimulator.Creator;
using LayoutSimulator.Enums;
using LayoutSimulator.Helpers;
using LayoutSimulator.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LayoutSimulator;

public class Layout : INotifyPropertyChanged
{
    public bool AutoMode { get; set; } = false;
    public Dictionary<string, Station> Stations { get; set; } = [];
    public Dictionary<string, Manipulator> Manipulators { get; set; } = [];
    public Dictionary<string, Reader> Readers { get; set; } = [];
    public Dictionary<int, Process> Processes { get; set; } = [];

    public List<Payload> WaitingPayloads { get; set; } = [];

    private LayoutState state = LayoutState.Stopped;
    public LayoutState State
    {
        get { return state; }
        set { state = value;
            Log.Instance.Info(new LogMessage($"Simulator State has changed to {value}."));
            OnPropertyChanged(); }
    }

    public Layout(bool autoMode)
    {
        AutoMode = autoMode;
    }

    public Pod CreatePod(string payloadType, int capacity, string startingPayloadState)
    {
        string podId = GenerateId.Instance.GetPodId();

        Cassette cassette = new()
        {
            PayloadType = payloadType,
            Capacity = capacity,
            IsMovableCassette = false
        };

        for (int i = 1; i <= capacity; i++)
        {
            Payload payload = new()
            {
                PayloadID = GenerateId.Instance.GetPayloadId(),
                LotID = podId,
                PayloadType = payloadType,
                SlotInLot = i,
                PayloadState = startingPayloadState,
                CurrentStationId = string.Empty,
            };
            payload.OnStateChanged += AddPayloadToWaitingList;
            cassette.AddPayloadWithoutReservation(i, payload);
        }

        Pod pod = new()
        {
            PodID = podId,
            Cassette = cassette
        };
        return pod;
    }

    public void AddProcess(ProcessStruct processStruct)
    {
        if (Processes.ContainsKey(processStruct.ProcessId))
            throw new ErrorResponse(EErrorCode.ProgramError, $"Process with ID {processStruct.ProcessId} already exists.");

        Processes.Add(processStruct.ProcessId, new Process()
        {
            ProcessName = processStruct.ProcessName,
            InputState = processStruct.InputState,
            OutputState = processStruct.OutputState,
            ProcessTime = processStruct.ProcessTime,
            NextLocation = processStruct.NextLocation
        });
    }

    public void AddStation(StationStruct stationStruct)
    {
        for(int i = 0; i < stationStruct.Count; i++)
        {
            int id = i;
            while (Stations.ContainsKey($"{stationStruct.Identifier}{id}"))
                id++;
            string stationId = $"{stationStruct.Identifier}{id}";

            Cassette? cassette = (stationStruct.IsInputAndPodDockable || stationStruct.IsOutputAndPodDockable) ? null : new Cassette()
            {
                PayloadType = stationStruct.PayloadType,
                Capacity = stationStruct.Capacity,
                IsMovableCassette = stationStruct.IsIndexable
            };

            Dictionary<string, Access> locations = [];
            for (int j = 0; j < stationStruct.AccessibleLocationsWithoutDoor.Count; j++)
            {
                locations.Add(stationStruct.AccessibleLocationsWithoutDoor[j], new Access(hasDoor: false, transitionTime: 0, accessiblePayloads: stationStruct.AccessiblePayloadsThroughtGap[j]));
            }

            if (stationStruct.AccessibleLocationsWithDoor.Count != stationStruct.DoorTransitionTimes.Count)
                throw new ErrorResponse(EErrorCode.MissingArguments, $"There are {stationStruct.AccessibleLocationsWithDoor.Count} in the station and {stationStruct.DoorTransitionTimes.Count} door transition times.");
            for (int j = 0; j < stationStruct.AccessibleLocationsWithDoor.Count; j++)
            {
                locations.Add(stationStruct.AccessibleLocationsWithDoor[j], new Access(hasDoor: true, transitionTime: stationStruct.DoorTransitionTimes[j], accessiblePayloads: stationStruct.AccessiblePayloadsThroughDoor[j]));
            }

            Dictionary<string, Process> stationProcesses = [];
            foreach (int processId in stationStruct.ProcessIds)
            {
                if (!Processes.ContainsKey(processId))
                    throw new ErrorResponse(EErrorCode.ProgramError, $"Process with ID {processId} does not exist.");
                stationProcesses.Add(Processes[processId].ProcessName, Processes[processId]);
            }

            Station station = new()
            {
                StationId = stationId,
                AutoMode = AutoMode,
                Cassette = cassette,
                IsInputAndPodDockable = stationStruct.IsInputAndPodDockable,
                IsOutputAndPodDockable = stationStruct.IsOutputAndPodDockable,
                Locations = locations,
                Processes = stationProcesses,
                Processable = stationStruct.Processable,
                HighPriority = stationStruct.HighPriority,
            };
            Stations.Add(stationId, station);
        }
    }

    public void AddManipulator(ManipulatorStruct manipulatorStruct)
    {
        for (int i = 0; i < manipulatorStruct.Count; i++)
        {
            int id = i;
            while (Manipulators.ContainsKey($"{manipulatorStruct.ManipulatorIdentifier}{id}"))
                id++;
            string manipulatorId = $"{manipulatorStruct.ManipulatorIdentifier}{id}";
            
            Dictionary<int, EndEffector> endEffectors = [];
            if( manipulatorStruct.EndEffectors.Count != manipulatorStruct.EndEffectorSlots.Count)
                throw new ErrorResponse(EErrorCode.MissingArguments, "EndEffectors and EndEffectorSlots count must be the same.");

            for (int j = 0; j < manipulatorStruct.EndEffectors.Count; j++)
            {
                EndEffector endEffector = new()
                {
                    PayloadType = manipulatorStruct.EndEffectors[j],
                    PayloadSlots = manipulatorStruct.EndEffectorSlots[j],
                };
                endEffectors.Add(j, endEffector);
            }

            Manipulator manipulator = new()
            {
                ManipulatorId = manipulatorId,
                EndEffectors = endEffectors,
                Locations = manipulatorStruct.Locations,
                MotionTime = (uint)manipulatorStruct.MotionTime,
                ExtendTime = (uint)manipulatorStruct.ExtendTime,
                RetractTime = (uint)manipulatorStruct.RetractTime,
            };
            Manipulators.Add(manipulatorId, manipulator);
        }
    }

    private void AddPayloadToWaitingList(object? sender, string e)
    {
        if (sender is not null and Payload)
        {
            Payload payload = (Payload)sender;
            if (!WaitingPayloads.Any(obj => obj.PayloadID == payload.PayloadID))
                WaitingPayloads.Add(payload);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
