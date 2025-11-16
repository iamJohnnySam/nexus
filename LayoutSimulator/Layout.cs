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
            foreach (string location in stationStruct.AccessibleLocationsWithoutDoor)
            {
                locations.Add(location, new Access(hasDoor: false, transitionTime: 0));
            }

            if (stationStruct.AccessibleLocationsWithDoor.Count != stationStruct.DoorTransitionTimes.Count)
                throw new ErrorResponse(EErrorCode.MissingArguments, $"There are {stationStruct.AccessibleLocationsWithDoor.Count} in the station and {stationStruct.DoorTransitionTimes.Count} door transition times.");
            for (i = 0; i < stationStruct.AccessibleLocationsWithDoor.Count; i++)
            {
                locations.Add(stationStruct.AccessibleLocationsWithDoor[i], new Access(hasDoor: true, transitionTime: stationStruct.DoorTransitionTimes[i]));
            }

            Station station = new()
            {
                StationId = stationId,
                AutoMode = AutoMode,
                Cassette = cassette,
                IsInputAndPodDockable = stationStruct.IsInputAndPodDockable,
                IsOutputAndPodDockable = stationStruct.IsOutputAndPodDockable,
                Locations = locations

            };
            Stations.Add(stationId, station);
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
