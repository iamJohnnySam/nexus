using LayoutSimulator.Commands;
using LayoutSimulator.Models;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Text;
using System.Xml.Linq;

namespace LayoutSimulator;

public class Layout
{
    public event EventHandler<LogMessage>? OnLogEvent;

    public ConcurrentDictionary<string, Pod> Pods { get; set; } = [];
    public Dictionary<string, Station> Stations { get; set; } = [];
    public Dictionary<string, Manipulator> Manipulators { get; set; } = [];
    public Dictionary<string, Reader> Readers { get; set; } = [];


    private LayoutState simState = LayoutState.Stopped;
    public LayoutState State
    {
        get
        {
            return simState;
        }
        set
        {
            simState = value;
            OnLogEvent?.Invoke(this, new LogMessage($"Simulator State has changed to {value}."));
        }
    }

    private const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public void InitializeLayout(string xmlPath, bool autoMode)
    {
        XDocument simDoc = XDocument.Load(xmlPath);
        CreateStations(simDoc.Descendants("Station"), autoMode);
        CreateManipulators(simDoc.Descendants("Manipulator"), autoMode);
        CreateReaders(simDoc.Descendants("Reader"));
    }

    public void CreateStations(IEnumerable<XElement> stations, bool autoMode)
    {
        foreach (var station in stations)
        {
            string identifier = station.Element("Identifier")?.Value ?? throw new ErrorResponse(EErrorCode.ProgramError, "No Station Identifier");
            identifier = identifier.Length >= 3 ? identifier.Substring(0, 3) : identifier;
            int count = int.Parse(station.Element("Count")?.Value ?? "1");
            string backUpProcessTime = station.Element("ProcessTime")?.Value ?? "5";

            string payloadType = station.Element("PayloadType")?.Value ?? "payload";
            int capacity = int.Parse(station.Element("Capacity")?.Value ?? "1");
            List<string> doorLocations = (station.Element("AccessibleLocationsWithDoor")?.Value ?? "").Split(',').Select(loc => loc.Trim()).ToList();
            List<string> noDoorLocations = (station.Element("AccessibleLocationsWitouthDoor")?.Value ?? "").Split(',').Select(loc => loc.Trim()).ToList();
            List<string> doorTransitionTimeString = (station.Element("DoorTransitionTimes")?.Value ?? "0").Split(',').Select(loc => loc.Trim()).ToList();
            List<float> doorTransitionTime = doorTransitionTimeString.Select(float.Parse).ToList();

            Dictionary<string, (string, string, string, float)> map = [];
            foreach (var process in station.Descendants("Process"))
            {
                if (process.Element("InputState") == null && process.Element("OutputState") == null)
                {
                    map.Add(process.Element("Name")?.Value ?? "process", (string.Empty, string.Empty,
                    process.Element("Location")?.Value ?? throw new ErrorResponse(EErrorCode.ProgramError, "Missing Location"),
                    float.Parse(process.Element("ProcessTime")?.Value ?? backUpProcessTime)));
                }
                else
                {
                    string pT = process.Element("ProcessTime")?.Value ?? backUpProcessTime;
                    if (pT == string.Empty)
                        pT = backUpProcessTime;
                    map.Add(process.Element("Name")?.Value.Trim() ?? "process", (
                    process.Element("InputState")?.Value.Trim() ?? throw new ErrorResponse(EErrorCode.ProgramError, "Missing Input State"),
                    process.Element("OutputState")?.Value.Trim() ?? throw new ErrorResponse(EErrorCode.ProgramError, "Missing Output State"),
                    process.Element("Location")?.Value.Trim() ?? throw new ErrorResponse(EErrorCode.ProgramError, "Missing Location"),
                    float.Parse(pT)));
                }
                if (!doorLocations.Contains(process.Element("Location")?.Value))
                {
                    if (!noDoorLocations.Contains(process.Element("Location")?.Value))
                    {
                        throw new ErrorResponse(EErrorCode.ProgramError, $"Location {process.Element("Location")?.Value} does not Exist");
                    }
                }
            }

            for (int i = 0; i < count; i++)
            {
                int j = i + 1;
                string stationName = $"{identifier}{j++}";
                while (Stations.ContainsKey(stationName))
                    stationName = $"{identifier}{j}";
                Stations.Add(stationName, new Station(
                    stationID: stationName.Trim(),
                    stationType: identifier.Trim(),
                    payloadType: payloadType.Trim(),
                    payloadStateMap: map,
                    capacity: capacity,
                    accessibleLocationsWithDoor: doorLocations,
                    accessibleLocationsWithoutDoor: noDoorLocations,
                    doorTransitionTime: doorTransitionTime,
                    concurrentLocationAccess: station.Element("ConcurrentLocationAccess")?.Value == "1",
                    processable: station.Element("Processable")?.Value == "1",
                    podDockable: station.Element("PodDockable")?.Value == "1",
                    autoLoadPod: station.Element("AutoLoadPod")?.Value == "1",
                    autoDoorControl: station.Element("AutoDoorControl")?.Value == "1",
                    lowPriority: station.Element("LowPriority")?.Value == "1",
                    partialProcess: station.Element("PartialProcess")?.Value == "1",
                    acceptedCommands: (station.Element("AcceptedCommands")?.Value ?? "").Split(',').Select(loc => loc.Trim()).ToList()));
                Stations[stationName].OnLogEvent += LogEvent;
                Stations[stationName].Tickable = !autoMode;
            }
        }
    }
    public void CreateManipulators(IEnumerable<XElement> manipulators, bool autoMode)
    {
        foreach (var manipulator in manipulators)
        {
            string identifier = manipulator.Element("Identifier")?.Value ?? "R";
            identifier = identifier.Length >= 3 ? identifier.Substring(0, 3) : identifier;
            List<string> endEffectorsTypes = (manipulator.Element("EndEffectors")?.Value ?? "payload").Split(',').Select(loc => loc.Trim()).ToList();
            List<string> locations = (manipulator.Element("Locations")?.Value ?? "location").Split(',').Select(loc => loc.Trim()).ToList();
            float motionTime = float.Parse(manipulator.Element("MotionTime")?.Value ?? "0");
            float extendTime = float.Parse(manipulator.Element("ExtendTime")?.Value ?? "0");
            float retractTime = float.Parse(manipulator.Element("RetractTime")?.Value ?? "0");
            int count = int.Parse(manipulator.Element("Count")?.Value ?? "0");

            Dictionary<int, Dictionary<string, Payload>> endEffectors = [];
            int endEffector = 1;
            foreach (string payload in endEffectorsTypes)
                endEffectors.Add(endEffector++, []);


            for (int i = 0; i < count; i++)
            {
                int j = i + 1;
                string manipulatornName = $"{identifier}{j++}";
                while (Manipulators.ContainsKey(manipulatornName))
                    manipulatornName = $"{identifier}{j}";
                Manipulators.Add(manipulatornName, new Manipulator(manipulatornName, identifier, endEffectors, endEffectorsTypes, locations, motionTime, extendTime, retractTime));
                Manipulators[manipulatornName].OnLogEvent += LogEvent;
                Manipulators[manipulatornName].Tickable = !autoMode;
            }
        }
    }
    public void CreateReaders(IEnumerable<XElement> readers)
    {
        foreach (var reader in readers)
        {
            string identifier = reader.Element("Identifier")?.Value ?? "B";
            string stationID = reader.Element("StationIdentifier")?.Value ?? "P";
            string type = reader.Element("Type")?.Value ?? "PAYLOAD";
            int slot = int.Parse(reader.Element("Slot")?.Value ?? "1");

            int j = 1;
            string targetStation = $"{stationID}{j}";

            while (Stations.ContainsKey(targetStation))
            {
                string readerName = $"{identifier}{j++}";
                if (type == "PAYLOAD")
                    Readers.Add(readerName, new Reader(readerName, identifier, Stations[targetStation], slot));
                else
                    Readers.Add(readerName, new Reader(readerName, identifier, Stations[targetStation]));
                j++;
                targetStation = $"{stationID}{j}";

            }
        }
    }
    public string CreatePod(Job command, int IDLength)
    {
        string podID = GetID(IDLength);
        CreatePod(command.TransactionID, podID, int.Parse(command.Arguments[(int)ECommandArgumentType.Capacity]), command.Arguments[(int)ECommandArgumentType.Type]);
        return podID;
    }
    public void CreatePod(string transactionID, string podID, int capacity, string payloadType)
    {
        Pods.TryAdd(podID, new Pod(podID, capacity, payloadType));
        OnLogEvent?.Invoke(this, new LogMessage(transactionID, $"Created Pod {podID} for {payloadType} with {capacity} slots."));
    }
    public Payload CreatePayload(Job command, int IDLength)
    {
        string payloadID = GetID(IDLength);
        Payload payload = CreatePayload(command.TransactionID, payloadID, command.Arguments[(int)ECommandArgumentType.PodId], int.Parse(command.Arguments[(int)ECommandArgumentType.Slot]));
        return payload;
    }
    public Payload CreatePayload(string transactionID, string payloadID, string podID, int slot)
    {
        Payload payload = new(payloadID, Pods[podID].PayloadType, podID, slot);
        Pods[podID].slots[slot] = payload;
        OnLogEvent?.Invoke(this, new LogMessage(transactionID, $"Created Payload {payloadID} on Pod {podID} at slot {slot}."));
        return payload;
    }

    private void CheckPodExist(string target)
    {
        if (!Pods.ContainsKey(target))
            throw new NAckResponse(ENAckCode.TargetNotExist, $"Could not find pod {target}.");
    }
    private void CheckStationExist(string target)
    {
        if (!Stations.ContainsKey(target))
            throw new NAckResponse(ENAckCode.TargetNotExist, $"Could not find station {target}.");
    }
    private void CheckManipulatorExist(string target)
    {
        if (!Manipulators.ContainsKey(target))
            throw new NAckResponse(ENAckCode.TargetNotExist, $"Could not find manipulator {target}.");
    }
    private void CheckReaderExist(string target)
    {
        if (!Readers.ContainsKey(target))
            throw new NAckResponse(ENAckCode.TargetNotExist, $"Could not find reader {target}.");
    }

    public void DockPod(Job command)
    {
        DockPod(command.TransactionID, command.Target, command.Arguments[(int)ECommandArgumentType.PodId]);
    }
    public void DockPod(string tID, string stationID, string podID)
    {
        Stations[stationID].Dock(tID, Pods[podID]);
        bool removed = Pods.TryRemove(podID, out _);
        while (!removed)
        {
            removed = Pods.TryRemove(podID, out _);
        }
    }

    public void UndockPod(Job command)
    {
        Pod outgoingPod = Stations[command.Target].UnDock(command.TransactionID);
        Pods.TryAdd(outgoingPod.PodID, outgoingPod);
    }
    public void UndockPod(string tID, string stationID)
    {
        Pod outgoingPod = Stations[stationID].UnDock(tID);
        bool added = Pods.TryAdd(outgoingPod.PodID, outgoingPod);
        while (!added)
        {
            added = Pods.TryAdd(outgoingPod.PodID, outgoingPod);
        }
    }

    public void ManipulatorPick(string tID, string manipulatorID, int EndEffector, string targetStationID, int slot)
    {
        Manipulators[manipulatorID].Pick(tID, EndEffector, Stations[targetStationID], slot);
    }

    public void ManipulatorPlace(string tID, string manipulatorID, int EndEffector, string targetStationID, int slot)
    {
        Manipulators[manipulatorID].Place(tID, EndEffector, Stations[targetStationID], slot);
    }

    public void ProcessStation(string tID, string stationID, string? process = null)
    {
        Stations[stationID].Process(tID, process);
    }
    public void ProcessStation(Job command)
    {
        Stations[command.Target].Process(command.TransactionID);
    }

    public void OperateStationDoor(string tID, string stationID, bool doorState)
    {
        Station station = Stations[stationID];
        if (station.Locations.Keys.Count == 1)
        {
            station.Door(tID, station.Locations.Keys.First(), doorState);
        }
        else
        {
            throw new ErrorResponse(EErrorCode.MissingArguments, "Missing location");
        }
    }
    public void OperateStationDoor(string tID, string stationID, string location, bool doorState)
    {
        Stations[stationID].Door(tID, location, doorState);
    }

    public static string GetID(int length)
    {
        StringBuilder val = new(length);
        for (int i = 0; i < length; i++)
            val.Append(validChars[new Random().Next(validChars.Length)]);
        return val.ToString();
    }


    public void CheckCommand(Job command, bool commandLock)
    {
        OnLogEvent?.Invoke(this, new LogMessage(command.TransactionID, $"Checking {command.Action} for {command.Target}"));

        if (State != LayoutState.ListeningCommands && command.Action != ECommandType.StartSim)
            throw new NAckResponse(ENAckCode.SimulatorNotStarted, $"Simulator not started.");

        switch (command.Action)
        {
            case ECommandType.Pick:
                CheckManipulatorExist(command.Target);
                CheckStationExist(command.Arguments[(int)ECommandArgumentType.TargetStation]);

                if (Manipulators[command.Target].State == EStationState.Off)
                    throw new NAckResponse(ENAckCode.PowerOff, $"Simulator caught Manipulator {command.Target} power off.");
                if (Manipulators[command.Target].State != EStationState.Idle)
                    throw new NAckResponse(ENAckCode.Busy, $"Simulator caught Manipulator {command.Target} busy.");
                if (!Manipulators[command.Target].EndEffectors.ContainsKey(int.Parse(command.Arguments[(int)ECommandArgumentType.EndEffector])))
                    throw new NAckResponse(ENAckCode.EndEffectorMissing, $"Simulator caught Manipulator {command.Target} does not have End Effector.");
                break;


            case ECommandType.Place:
                CheckManipulatorExist(command.Target);
                CheckStationExist(command.Arguments[(int)ECommandArgumentType.TargetStation]);

                if (Manipulators[command.Target].State == EStationState.Off)
                    throw new NAckResponse(ENAckCode.PowerOff, $"Simulator caught Manipulator {command.Target} power off.");
                if (Manipulators[command.Target].State != EStationState.Idle)
                    throw new NAckResponse(ENAckCode.Busy, $"Simulator caught Manipulator {command.Target} busy.");
                if (!Manipulators[command.Target].EndEffectors.ContainsKey(int.Parse(command.Arguments[(int)ECommandArgumentType.EndEffector])))
                    throw new NAckResponse(ENAckCode.EndEffectorMissing, $"Simulator caught Manipulator {command.Target} does not have End Effector.");
                break;


            case ECommandType.Door:
                CheckStationExist(command.Target);

                if (!Stations[command.Target].AcceptedCommands.Contains(command.RawAction) && commandLock)
                    throw new NAckResponse(ENAckCode.CommandError, $"Simulator could not find {command.RawAction} in accepted list of commands for this station.");
                if (Stations[command.Target].State != EStationState.Idle)
                    throw new NAckResponse(ENAckCode.Busy, $"Simulator caught Station {command.Target} busy.");
                // todo:
                throw new NotImplementedException();
                // if (!Stations[command.Target].HasDoor)
                //     throw new NackResponse(NackCodes.StationDoesNotHaveDoor, $"Simulator caught Station {command.Target} does not have a door.");
                break;

            case ECommandType.DoorOpen:
                CheckStationExist(command.Target);

                if (!Stations[command.Target].AcceptedCommands.Contains(command.RawAction) && commandLock)
                    throw new NAckResponse(ENAckCode.CommandError, $"Simulator could not find {command.RawAction} in accepted list of commands for this station.");
                if (Stations[command.Target].State != EStationState.Idle)
                    throw new NAckResponse(ENAckCode.Busy, $"Simulator caught Station {command.Target} busy.");
                // todo:
                throw new NotImplementedException();
                // if (!stations[command.target].hasdoor)
                //     throw new nackresponse(nackcodes.stationdoesnothavedoor, $"simulator caught station {command.target} does not have a door.");
                break;


            case ECommandType.DoorClose:
                CheckStationExist(command.Target);

                if (!Stations[command.Target].AcceptedCommands.Contains(command.RawAction) && commandLock)
                    throw new NAckResponse(ENAckCode.CommandError, $"Simulator could not find {command.RawAction} in accepted list of commands for this station.");
                if (Stations[command.Target].State != EStationState.Idle)
                    throw new NAckResponse(ENAckCode.Busy, $"Simulator caught Station {command.Target} busy.");
                // todo:
                throw new NotImplementedException();
                // if (!Stations[command.Target].HasDoor)
                // throw new NackResponse(NackCodes.StationDoesNotHaveDoor, $"Simulator caught Station {command.Target} does not have a door.");
                break;


            case ECommandType.Map:
                CheckStationExist(command.Target);

                if (!Stations[command.Target].AcceptedCommands.Contains(command.RawAction) && commandLock)
                    throw new NAckResponse(ENAckCode.CommandError, $"Simulator could not find {command.RawAction} in accepted list of commands for this station.");
                if (Stations[command.Target].State != EStationState.Idle)
                    throw new NAckResponse(ENAckCode.Busy, $"Simulator caught Station {command.Target} busy.");
                if (!Stations[command.Target].Mappable)
                    throw new NAckResponse(ENAckCode.NotMappable, $"Simulator caught Station {command.Target} not mappable.");
                break;


            case ECommandType.Dock:
                if (!command.Arguments.ContainsKey((int)ECommandArgumentType.PodId) || command.Arguments[(int)ECommandArgumentType.PodId] == "")
                {
                    command.Arguments[(int)ECommandArgumentType.PodId] = Pods.Keys.Last();
                    OnLogEvent?.Invoke(this, new LogMessage(command.TransactionID, $"Chnaged pod ID to {Pods.Keys.Last()}"));
                }

                CheckStationExist(command.Target);
                CheckPodExist(command.Arguments[(int)ECommandArgumentType.PodId]);

                if (!Stations[command.Target].AcceptedCommands.Contains(command.RawAction) && commandLock)
                    throw new NAckResponse(ENAckCode.CommandError, $"Simulator could not find {command.RawAction} in accepted list of commands for this station.");
                if (Stations[command.Target].State != EStationState.Idle)
                    throw new NAckResponse(ENAckCode.Busy, $"Simulator caught Station {command.Target} busy.");
                if (!Stations[command.Target].PodDockable)
                    throw new NAckResponse(ENAckCode.NotDockable, $"Simulator caught Station {command.Target} not dockable.");
                break;


            case ECommandType.Undock:
                CheckStationExist(command.Target);

                if (!Stations[command.Target].AcceptedCommands.Contains(command.RawAction) && commandLock)
                    throw new NAckResponse(ENAckCode.CommandError, $"Simulator could not find {command.RawAction} in accepted list of commands for this station.");
                if (Stations[command.Target].State != EStationState.Idle)
                    throw new NAckResponse(ENAckCode.Busy, $"Simulator caught Station {command.Target} busy.");
                if (!Stations[command.Target].PodDockable)
                    throw new NAckResponse(ENAckCode.NotDockable, $"Simulator caught Station {command.Target} not dockable.");
                break;

            case ECommandType.Process0:
            case ECommandType.Process1:
            case ECommandType.Process2:
            case ECommandType.Process3:
            case ECommandType.Process4:
            case ECommandType.Process5:
            case ECommandType.Process6:
            case ECommandType.Process7:
            case ECommandType.Process8:
            case ECommandType.Process9:
                CheckStationExist(command.Target);

                if (!Stations[command.Target].AcceptedCommands.Contains(command.RawAction) && commandLock)
                    throw new NAckResponse(ENAckCode.CommandError, $"Simulator could not find {command.RawAction} in accepted list of commands for this station.");
                if (Stations[command.Target].State != EStationState.Idle)
                    throw new NAckResponse(ENAckCode.Busy, $"Simulator caught Station {command.Target} busy.");
                break;


            case ECommandType.Power:
            case ECommandType.PowerOn:
            case ECommandType.PowerOff:
                CheckManipulatorExist(command.Target);
                break;


            case ECommandType.Home:
                if (Manipulators.TryGetValue(command.Target, out Manipulator? manipulator))
                {
                    if (manipulator.State == EStationState.Off)
                        throw new NAckResponse(ENAckCode.PowerOff, $"Simulator caught Manipulator {command.Target} power off.");
                    if (manipulator.State != EStationState.Idle)
                        throw new NAckResponse(ENAckCode.Busy, $"Simulator caught Manipulator {command.Target} busy.");
                }
                else if (Stations.TryGetValue(command.Target, out Station? station))
                {
                    if (station.State != EStationState.Idle)
                        throw new NAckResponse(ENAckCode.Busy, $"Simulator caught Station {command.Target} busy.");
                    if (!station.HasDoors)
                        throw new NAckResponse(ENAckCode.StationDoesNotHaveDoor, $"Simulator caught Station {command.Target} does not have door.");
                }
                else
                {
                    throw new NAckResponse(ENAckCode.CommandError, $"Simulator could not find station {command.Target}");
                }
                break;


            case ECommandType.ReadPod:
            case ECommandType.ReadSlot:
                CheckReaderExist(command.Target);
                break;


            case ECommandType.Pod:
                break;


            case ECommandType.Payload:
                if (!command.Arguments.ContainsKey((int)ECommandArgumentType.PodId) || command.Arguments[(int)ECommandArgumentType.PodId] == "")
                {
                    command.Arguments[(int)ECommandArgumentType.PodId] = Pods.Keys.Last();
                    OnLogEvent?.Invoke(this, new LogMessage(command.TransactionID, $"Chnaged pod ID to {Pods.Keys.Last()}"));
                }

                CheckPodExist(command.Arguments[(int)ECommandArgumentType.PodId]);

                int slot = int.Parse(command.Arguments[(int)ECommandArgumentType.Slot]);

                if (slot < 1)
                    throw new NAckResponse(ENAckCode.CommandError, $"Simulator cannot create payload in slot less than 1.");

                while (Pods[command.Arguments[(int)ECommandArgumentType.PodId]].slots.ContainsKey(slot))
                {
                    slot++;
                    if (slot > Pods[command.Arguments[(int)ECommandArgumentType.PodId]].Capacity)
                        throw new NAckResponse(ENAckCode.CommandError, $"Simulator cannot create payload in slot greater than station capacity.");

                    command.Arguments[(int)ECommandArgumentType.Slot] = slot.ToString();
                }
                break;
        }
    }
    public string ExecuteCommand(Job command, string response)
    {
        while (State == LayoutState.Paused)
        {
            Thread.Sleep(100);
        }

        if (State == LayoutState.Paused)
            throw new ErrorResponse(EErrorCode.SimulatorStopped, $"Simulator was stopped.");

        OnLogEvent?.Invoke(this, new LogMessage(command.TransactionID, $"Processing {command.Action} for {command.Target}"));
        switch (command.Action)
        {
            case ECommandType.Pick:
                ManipulatorPick(command.TransactionID, command.Target, int.Parse(command.Arguments[(int)ECommandArgumentType.EndEffector]), command.Arguments[(int)ECommandArgumentType.TargetStation], int.Parse(command.Arguments[(int)ECommandArgumentType.Slot]));
                break;

            case ECommandType.Place:
                ManipulatorPlace(command.TransactionID, command.Target, int.Parse(command.Arguments[(int)ECommandArgumentType.EndEffector]), command.Arguments[(int)ECommandArgumentType.TargetStation], int.Parse(command.Arguments[(int)ECommandArgumentType.Slot]));
                break;

            case ECommandType.Door:
                OperateStationDoor(command.TransactionID, command.Target, ConvertStringtoBool(command.Arguments[(int)ECommandArgumentType.DoorStatus]));
                break;

            case ECommandType.DoorOpen:
                OperateStationDoor(command.TransactionID, command.Target, false);
                break;


            case ECommandType.DoorClose:
                OperateStationDoor(command.TransactionID, command.Target, true);
                break;

            case ECommandType.Map:
                List<int> mapData = Stations[command.Target].OpenDoorAndMap(command.TransactionID).Cast<int>().ToList();
                response = string.Join("", mapData);
                break;

            case ECommandType.Dock:
                DockPod(command);
                break;

            case ECommandType.Undock:
                UndockPod(command);
                break;

            case ECommandType.Process0:
            case ECommandType.Process1:
            case ECommandType.Process2:
            case ECommandType.Process3:
            case ECommandType.Process4:
            case ECommandType.Process5:
            case ECommandType.Process6:
            case ECommandType.Process7:
            case ECommandType.Process8:
            case ECommandType.Process9:
                ProcessStation(command);
                break;

            case ECommandType.Power:
                if (ConvertStringtoBool(command.Arguments[(int)ECommandArgumentType.PowerStatus]))
                {
                    command.Action = ECommandType.PowerOn;
                    Manipulators[command.Target].PowerOn(command.TransactionID);
                }
                else
                {
                    command.Action = ECommandType.PowerOff;
                    Manipulators[command.Target].PowerOff(command.TransactionID);
                }
                break;

            case ECommandType.PowerOn:
                Manipulators[command.Target].PowerOn(command.TransactionID);
                break;

            case ECommandType.PowerOff:
                Manipulators[command.Target].PowerOff(command.TransactionID);
                break;

            case ECommandType.Home:
                if (Manipulators.TryGetValue(command.Target, out Manipulator? manipulator))
                    manipulator.Home(command.TransactionID);
                else if (Stations.TryGetValue(command.Target, out Station? station))
                    OperateStationDoor(command.TransactionID, station.StationID, true);

                else
                    throw new ErrorResponse(EErrorCode.ProgramError, $"Simulator could not identify target {command.Target} to home.");
                break;


            case ECommandType.ReadPod:
            case ECommandType.ReadSlot:
                response = Readers[command.Target].ReadID(command.TransactionID);
                break;

            case ECommandType.Pod:
                response = CreatePod(command, 5);
                break;

            case ECommandType.Payload:
                response = CreatePayload(command, 5).PayloadID;
                break;

            case ECommandType.StartSim:
            case ECommandType.ResumeSim:
                State = LayoutState.ListeningCommands;
                break;

            case ECommandType.StopSim:
                State = LayoutState.Stopped;
                break;

            case ECommandType.PauseSim:
                State = LayoutState.Paused;
                break;
        }
        return response;
    }

    private static bool ConvertStringtoBool(string str)
    {
        if (int.Parse(str) == 0)
            return false;
        return true;
    }

    private void LogEvent(object? sender, LogMessage e)
    {
        OnLogEvent?.Invoke(sender, e);
    }
}
