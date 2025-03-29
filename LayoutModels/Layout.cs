using LayoutModels.Manipulators;
using LayoutModels.Readers;
using LayoutModels.Stations;
using Logger;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;

namespace LayoutModels
{
    public enum LayoutState
    {
        ListeningCommands,
        Stopped,
        AutoRun,
        Paused,
    }

    public class Layout
    {
        public event EventHandler<(string? tID, string message)>? OnLogEvent;

        public ConcurrentDictionary<string, Pod> Pods { get; set; } = [];
        public Dictionary<string, Station> StationList { get; set; } = [];
        public Dictionary<string, Manipulator> ManipulatorList { get; set; } = [];
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
                OnLogEvent?.Invoke(this, (null, $"Simulator State has changed to {value}."));
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
                string identifier = station.Element("Identifier")?.Value ?? throw new ErrorResponse(ErrorCodes.ProgramError, "No Station Identifier");
                int count = int.Parse(station.Element("Count")?.Value ?? "1");
                string backUpProcessTime = station.Element("ProcessTime")?.Value ?? "5";

                Dictionary<string, (string, string, string, float)> map = [];
                foreach (var process in station.Descendants("Process"))
                {
                    if (process.Element("InputState") == null && process.Element("OutputState") == null)
                    {
                        map.Add(process.Element("Name")?.Value ?? "process",(string.Empty, string.Empty,
                        process.Element("Location")?.Value ?? throw new ErrorResponse(ErrorCodes.ProgramError, "Missing Location"),
                        float.Parse(process.Element("ProcessTime")?.Value ?? backUpProcessTime)));
                    }
                    else
                    {
                        map.Add(process.Element("Name")?.Value ?? "process", (
                        process.Element("InputState")?.Value ?? throw new ErrorResponse(ErrorCodes.ProgramError, "Missing Input State"),
                        process.Element("OutputState")?.Value ?? throw new ErrorResponse(ErrorCodes.ProgramError, "Missing Output State"),
                        process.Element("Location")?.Value ?? throw new ErrorResponse(ErrorCodes.ProgramError, "Missing Location"),
                        float.Parse(process.Element("ProcessTime")?.Value ?? backUpProcessTime)));
                    }
                }

                string payloadType = station.Element("PayloadType")?.Value ?? "payload";
                int capacity = int.Parse(station.Element("Capacity")?.Value ?? "1");
                List<string> doorLocations = (station.Element("AccessibleLocationsWithDoor")?.Value ?? "").Split(',').Select(loc => loc.Trim()).ToList();
                List<string> noDoorLocations = (station.Element("AccessibleLocationsWitouthDoor")?.Value ?? "").Split(',').Select(loc => loc.Trim()).ToList();
                List<string> doorTransitionTimeString = (station.Element("DoorTransitionTimes")?.Value ?? "0").Split(',').Select(loc => loc.Trim()).ToList();
                List<float> doorTransitionTime = doorTransitionTimeString.Select(float.Parse).ToList();

                for (int i = 0; i < count; i++)
                {
                    int j = i + 1;
                    string stationName = $"{identifier}{j++}";
                    while (StationList.ContainsKey(stationName))
                        stationName = $"{identifier}{j}";
                    StationList.Add(stationName, new Station(
                        stationID: stationName,
                        stationType: identifier,
                        payloadType: payloadType,
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
                    StationList[stationName].OnLogEvent += LogEvent;
                    StationList[stationName].Tickable = !autoMode;
                }
            }
        }
        public void CreateManipulators(IEnumerable<XElement> manipulators, bool autoMode)
        {
            foreach (var manipulator in manipulators)
            {
                string identifier = manipulator.Element("Identifier")?.Value ?? "R";
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
                    while (ManipulatorList.ContainsKey(manipulatornName))
                        manipulatornName = $"{identifier}{j}";
                    ManipulatorList.Add(manipulatornName, new Manipulator(manipulatornName, identifier, endEffectors, endEffectorsTypes, locations, motionTime, extendTime, retractTime));
                    ManipulatorList[manipulatornName].OnLogEvent += LogEvent;
                    ManipulatorList[manipulatornName].Tickable = !autoMode;
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

                while (StationList.ContainsKey(targetStation))
                {
                    string readerName = $"{identifier}{j++}";
                    if (type == "PAYLOAD")
                        Readers.Add(readerName, new Reader(readerName, identifier, StationList[targetStation], slot));
                    else
                        Readers.Add(readerName, new Reader(readerName, identifier, StationList[targetStation]));
                    j++;
                    targetStation = $"{stationID}{j}";

                }
            }
        }
        public string CreatePod(Job command, int IDLength)
        {
            string podID = GetID(IDLength);
            CreatePod(command.TransactionID, podID, Int32.Parse(command.Arguments[(int)CommandArgType.Capacity]), command.Arguments[(int)CommandArgType.Type]);
            return podID;
        }
        public void CreatePod(string transactionID, string podID, int capacity, string payloadType)
        {
            Pods.TryAdd(podID, new Pod(podID, capacity, payloadType));
            OnLogEvent?.Invoke(this, (transactionID, $"Created Pod {podID} for {payloadType} with {capacity} slots."));
        }
        public Payload CreatePayload(Job command, int IDLength)
        {
            string payloadID = GetID(IDLength);
            Payload payload = CreatePayload(command.TransactionID, payloadID, command.Arguments[(int)CommandArgType.PodId], Int32.Parse(command.Arguments[(int)CommandArgType.Slot]));
            return payload;
        }
        public Payload CreatePayload(string transactionID, string payloadID, string podID, int slot)
        {
            Payload payload = new(payloadID, Pods[podID].PayloadType, podID, slot);
            Pods[podID].slots[slot] = payload;
            OnLogEvent?.Invoke(this, (transactionID, $"Created Payload {payloadID} on Pod {podID} at slot {slot}."));
            return payload;
        }

        private void CheckPodExist(string target)
        {
            if (!Pods.ContainsKey(target))
                throw new NAckResponse(NAckCodes.TargetNotExist, $"Could not find pod {target}.");
        }
        private void CheckStationExist(string target)
        {
            if (!StationList.ContainsKey(target))
                throw new NAckResponse(NAckCodes.TargetNotExist, $"Could not find station {target}.");
        }
        private void CheckManipulatorExist(string target)
        {
            if (!ManipulatorList.ContainsKey(target))
                throw new NAckResponse(NAckCodes.TargetNotExist, $"Could not find manipulator {target}.");
        }
        private void CheckReaderExist(string target)
        {
            if (!Readers.ContainsKey(target))
                throw new NAckResponse(NAckCodes.TargetNotExist, $"Could not find reader {target}.");
        }

        public void DockPod(Job command)
        {
            DockPod(command.TransactionID, command.Target, command.Arguments[(int)CommandArgType.PodId]);
        }
        public void DockPod(string tID, string stationID, string podID)
        {
            StationList[stationID].Dock(tID, Pods[podID]);
            bool removed = Pods.TryRemove(podID, out _);
            while (!removed)
            {
                removed = Pods.TryRemove(podID, out _);
            }
        }

        public void UndockPod(Job command)
        {
            Pod outgoingPod = StationList[command.Target].UnDock(command.TransactionID);
            Pods.TryAdd(outgoingPod.PodID, outgoingPod);
        }
        public void UndockPod(string tID, string stationID)
        {
            Pod outgoingPod = StationList[stationID].UnDock(tID);
            bool added = Pods.TryAdd(outgoingPod.PodID, outgoingPod);
            while (!added)
            {
                added = Pods.TryAdd(outgoingPod.PodID, outgoingPod);
            }
        }

        public void ManipulatorPick(string tID, string manipulatorID, int EndEffector, string targetStationID, int slot)
        {
            ManipulatorList[manipulatorID].Pick(tID, EndEffector, StationList[targetStationID], slot);
        } 
        
        public void ManipulatorPlace(string tID, string manipulatorID, int EndEffector, string targetStationID, int slot)
        {
            ManipulatorList[manipulatorID].Place(tID, EndEffector, StationList[targetStationID], slot);
        }

        public void ProcessStation(string tID, string stationID, string? process = null)
        {
            StationList[stationID].Process(tID, process);
        }
        public void ProcessStation(Job command)
        {
            StationList[command.Target].Process(command.TransactionID);
        }

        public void OperateStationDoor(string tID, string stationID, bool doorState)
        {
            Station station = StationList[stationID];
            if (station.Locations.Keys.Count == 1)
            {
                station.Door(tID, station.Locations.Keys.First(), doorState);
            }
            else
            {
                throw new ErrorResponse(ErrorCodes.MissingArguments, "Missing location");
            }
        }
        public void OperateStationDoor(string tID, string stationID, string location, bool doorState)
        {
            StationList[stationID].Door(tID, location, doorState);
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
            OnLogEvent?.Invoke(this, (command.TransactionID, $"Checking {command.Action} for {command.Target}"));

            if (State != LayoutState.ListeningCommands && command.Action != CommandType.StartSim)
                throw new NAckResponse(NAckCodes.SimulatorNotStarted, $"Simulator not started.");

            switch (command.Action)
            {
                case CommandType.Pick:
                    CheckManipulatorExist(command.Target);
                    CheckStationExist(command.Arguments[(int)CommandArgType.TargetStation]);

                    if (ManipulatorList[command.Target].State == StationState.Off)
                        throw new NAckResponse(NAckCodes.PowerOff, $"Simulator caught Manipulator {command.Target} power off.");
                    if (ManipulatorList[command.Target].State != StationState.Idle)
                        throw new NAckResponse(NAckCodes.Busy, $"Simulator caught Manipulator {command.Target} busy.");
                    if (!ManipulatorList[command.Target].EndEffectors.ContainsKey(Int32.Parse(command.Arguments[(int)CommandArgType.EndEffector])))
                        throw new NAckResponse(NAckCodes.EndEffectorMissing, $"Simulator caught Manipulator {command.Target} does not have End Effector.");
                    break;


                case CommandType.Place:
                    CheckManipulatorExist(command.Target);
                    CheckStationExist(command.Arguments[(int)CommandArgType.TargetStation]);

                    if (ManipulatorList[command.Target].State == StationState.Off)
                        throw new NAckResponse(NAckCodes.PowerOff, $"Simulator caught Manipulator {command.Target} power off.");
                    if (ManipulatorList[command.Target].State != StationState.Idle)
                        throw new NAckResponse(NAckCodes.Busy, $"Simulator caught Manipulator {command.Target} busy.");
                    if (!ManipulatorList[command.Target].EndEffectors.ContainsKey(Int32.Parse(command.Arguments[(int)CommandArgType.EndEffector])))
                        throw new NAckResponse(NAckCodes.EndEffectorMissing, $"Simulator caught Manipulator {command.Target} does not have End Effector.");
                    break;


                case CommandType.Door:
                    CheckStationExist(command.Target);

                    if (!StationList[command.Target].AcceptedCommands.Contains(command.RawAction) && commandLock)
                        throw new NAckResponse(NAckCodes.CommandError, $"Simulator could not find {command.RawAction} in accepted list of commands for this station.");
                    if (StationList[command.Target].State != StationState.Idle)
                        throw new NAckResponse(NAckCodes.Busy, $"Simulator caught Station {command.Target} busy.");
                    // todo:
                    throw new NotImplementedException();
                    // if (!Stations[command.Target].HasDoor)
                    //     throw new NAckResponse(NAckCodes.StationDoesNotHaveDoor, $"Simulator caught Station {command.Target} does not have a door.");
                    break;

                case CommandType.DoorOpen:
                    CheckStationExist(command.Target);

                    if (!StationList[command.Target].AcceptedCommands.Contains(command.RawAction) && commandLock)
                        throw new NAckResponse(NAckCodes.CommandError, $"Simulator could not find {command.RawAction} in accepted list of commands for this station.");
                    if (StationList[command.Target].State != StationState.Idle)
                        throw new NAckResponse(NAckCodes.Busy, $"Simulator caught Station {command.Target} busy.");
                    // todo:
                    throw new NotImplementedException();
                    // if (!stations[command.target].hasdoor)
                    //     throw new NAckresponse(nackcodes.stationdoesnothavedoor, $"simulator caught station {command.target} does not have a door.");
                    break;


                case CommandType.DoorClose:
                    CheckStationExist(command.Target);

                    if (!StationList[command.Target].AcceptedCommands.Contains(command.RawAction) && commandLock)
                        throw new NAckResponse(NAckCodes.CommandError, $"Simulator could not find {command.RawAction} in accepted list of commands for this station.");
                    if (StationList[command.Target].State != StationState.Idle)
                        throw new NAckResponse(NAckCodes.Busy, $"Simulator caught Station {command.Target} busy.");
                    // todo:
                    throw new NotImplementedException();
                    // if (!Stations[command.Target].HasDoor)
                    // throw new NAckResponse(NAckCodes.StationDoesNotHaveDoor, $"Simulator caught Station {command.Target} does not have a door.");
                    break;


                case CommandType.Map:
                    CheckStationExist(command.Target);

                    if (!StationList[command.Target].AcceptedCommands.Contains(command.RawAction) && commandLock)
                        throw new NAckResponse(NAckCodes.CommandError, $"Simulator could not find {command.RawAction} in accepted list of commands for this station.");
                    if (StationList[command.Target].State != StationState.Idle)
                        throw new NAckResponse(NAckCodes.Busy, $"Simulator caught Station {command.Target} busy.");
                    if (!StationList[command.Target].Mappable)
                        throw new NAckResponse(NAckCodes.NotMappable, $"Simulator caught Station {command.Target} not mappable.");
                    break;


                case CommandType.Dock:
                    if (!command.Arguments.ContainsKey((int)CommandArgType.PodId) || (command.Arguments[(int)CommandArgType.PodId] == ""))
                    {
                        command.Arguments[(int)CommandArgType.PodId] = Pods.Keys.Last();
                        OnLogEvent?.Invoke(this, (command.TransactionID, $"Chnaged pod ID to {Pods.Keys.Last()}"));
                    }

                    CheckStationExist(command.Target);
                    CheckPodExist(command.Arguments[(int)CommandArgType.PodId]);

                    if (!StationList[command.Target].AcceptedCommands.Contains(command.RawAction) && commandLock)
                        throw new NAckResponse(NAckCodes.CommandError, $"Simulator could not find {command.RawAction} in accepted list of commands for this station.");
                    if (StationList[command.Target].State != StationState.Idle)
                        throw new NAckResponse(NAckCodes.Busy, $"Simulator caught Station {command.Target} busy.");
                    if (!StationList[command.Target].PodDockable)
                        throw new NAckResponse(NAckCodes.NotDockable, $"Simulator caught Station {command.Target} not dockable.");
                    break;


                case CommandType.Undock:
                    CheckStationExist(command.Target);

                    if (!StationList[command.Target].AcceptedCommands.Contains(command.RawAction) && commandLock)
                        throw new NAckResponse(NAckCodes.CommandError, $"Simulator could not find {command.RawAction} in accepted list of commands for this station.");
                    if (StationList[command.Target].State != StationState.Idle)
                        throw new NAckResponse(NAckCodes.Busy, $"Simulator caught Station {command.Target} busy.");
                    if (!StationList[command.Target].PodDockable)
                        throw new NAckResponse(NAckCodes.NotDockable, $"Simulator caught Station {command.Target} not dockable.");
                    break;

                case CommandType.Process0:
                case CommandType.Process1:
                case CommandType.Process2:
                case CommandType.Process3:
                case CommandType.Process4:
                case CommandType.Process5:
                case CommandType.Process6:
                case CommandType.Process7:
                case CommandType.Process8:
                case CommandType.Process9:
                    CheckStationExist(command.Target);

                    if (!StationList[command.Target].AcceptedCommands.Contains(command.RawAction) && commandLock)
                        throw new NAckResponse(NAckCodes.CommandError, $"Simulator could not find {command.RawAction} in accepted list of commands for this station.");
                    if (StationList[command.Target].State != StationState.Idle)
                        throw new NAckResponse(NAckCodes.Busy, $"Simulator caught Station {command.Target} busy.");
                    break;


                case CommandType.Power:
                case CommandType.PowerOn:
                case CommandType.PowerOff:
                    CheckManipulatorExist(command.Target);
                    break;


                case CommandType.Home:
                    if (ManipulatorList.TryGetValue(command.Target, out Manipulator? manipulator))
                    {
                        if (manipulator.State == StationState.Off)
                            throw new NAckResponse(NAckCodes.PowerOff, $"Simulator caught Manipulator {command.Target} power off.");
                        if (manipulator.State != StationState.Idle)
                            throw new NAckResponse(NAckCodes.Busy, $"Simulator caught Manipulator {command.Target} busy.");
                    }
                    else if (StationList.TryGetValue(command.Target, out Station? station))
                    {
                        if (station.State != StationState.Idle)
                            throw new NAckResponse(NAckCodes.Busy, $"Simulator caught Station {command.Target} busy.");
                        if (!station.HasDoors)
                            throw new NAckResponse(NAckCodes.StationDoesNotHaveDoor, $"Simulator caught Station {command.Target} does not have door.");
                    }
                    else
                    {
                        throw new NAckResponse(NAckCodes.CommandError, $"Simulator could not find station {command.Target}");
                    }
                    break;


                case CommandType.ReadPod:
                case CommandType.ReadSlot:
                    CheckReaderExist(command.Target);
                    break;


                case CommandType.Pod:
                    break;


                case CommandType.Payload:
                    if (!command.Arguments.ContainsKey((int)CommandArgType.PodId) || (command.Arguments[(int)CommandArgType.PodId] == ""))
                    {
                        command.Arguments[(int)CommandArgType.PodId] = Pods.Keys.Last();
                        OnLogEvent?.Invoke(this, (command.TransactionID, $"Chnaged pod ID to {Pods.Keys.Last()}"));
                    }

                    CheckPodExist(command.Arguments[(int)CommandArgType.PodId]);

                    int slot = Int32.Parse(command.Arguments[(int)CommandArgType.Slot]);

                    if (slot < 1)
                        throw new NAckResponse(NAckCodes.CommandError, $"Simulator cannot create payload in slot less than 1.");

                    while (Pods[command.Arguments[(int)CommandArgType.PodId]].slots.ContainsKey(slot))
                    {
                        slot++;
                        if (slot > Pods[command.Arguments[(int)CommandArgType.PodId]].Capacity)
                            throw new NAckResponse(NAckCodes.CommandError, $"Simulator cannot create payload in slot greater than station capacity.");

                        command.Arguments[(int)CommandArgType.Slot] = slot.ToString();
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
                throw new ErrorResponse(ErrorCodes.SimulatorStopped, $"Simulator was stopped.");

            OnLogEvent?.Invoke(this, (command.TransactionID, $"Processing {command.Action} for {command.Target}"));
            switch (command.Action)
            {
                case CommandType.Pick:
                    ManipulatorPick(command.TransactionID, command.Target, Int32.Parse(command.Arguments[(int)CommandArgType.EndEffector]), command.Arguments[(int)CommandArgType.TargetStation], Int32.Parse(command.Arguments[(int)CommandArgType.Slot]));
                    break;

                case CommandType.Place:
                    ManipulatorPlace(command.TransactionID, command.Target, Int32.Parse(command.Arguments[(int)CommandArgType.EndEffector]), command.Arguments[(int)CommandArgType.TargetStation], Int32.Parse(command.Arguments[(int)CommandArgType.Slot]));
                    break;

                case CommandType.Door:
                    OperateStationDoor(command.TransactionID, command.Target, ConvertStringtoBool(command.Arguments[(int)CommandArgType.DoorStatus]));
                    break;

                case CommandType.DoorOpen:
                    OperateStationDoor(command.TransactionID, command.Target, false);
                    break;


                case CommandType.DoorClose:
                    OperateStationDoor(command.TransactionID, command.Target, true);
                    break;

                case CommandType.Map:
                    List<int> mapData = StationList[command.Target].OpenDoorAndMap(command.TransactionID).Cast<int>().ToList();
                    response = string.Join("", mapData);
                    break;

                case CommandType.Dock:
                    DockPod(command);
                    break;

                case CommandType.Undock:
                    UndockPod(command);
                    break;

                case CommandType.Process0:
                case CommandType.Process1:
                case CommandType.Process2:
                case CommandType.Process3:
                case CommandType.Process4:
                case CommandType.Process5:
                case CommandType.Process6:
                case CommandType.Process7:
                case CommandType.Process8:
                case CommandType.Process9:
                    ProcessStation(command);
                    break;

                case CommandType.Power:
                    if (ConvertStringtoBool(command.Arguments[(int)CommandArgType.PowerStatus]))
                    {
                        command.Action = CommandType.PowerOn;
                        ManipulatorList[command.Target].PowerOn(command.TransactionID);
                    }
                    else
                    {
                        command.Action = CommandType.PowerOff;
                        ManipulatorList[command.Target].PowerOff(command.TransactionID);
                    }
                    break;

                case CommandType.PowerOn:
                    ManipulatorList[command.Target].PowerOn(command.TransactionID);
                    break;

                case CommandType.PowerOff:
                    ManipulatorList[command.Target].PowerOff(command.TransactionID);
                    break;

                case CommandType.Home:
                    if (ManipulatorList.TryGetValue(command.Target, out Manipulator? manipulator))
                        manipulator.Home(command.TransactionID);
                    else if (StationList.TryGetValue(command.Target, out Station? station))
                        OperateStationDoor(command.TransactionID, station.StationID, true);

                    else
                        throw new ErrorResponse(ErrorCodes.ProgramError, $"Simulator could not identify target {command.Target} to home.");
                    break;


                case CommandType.ReadPod:
                case CommandType.ReadSlot:
                    response = Readers[command.Target].ReadID(command.TransactionID);
                    break;

                case CommandType.Pod:
                    response = CreatePod(command, 5);
                    break;

                case CommandType.Payload:
                    response = CreatePayload(command, 5).PayloadID;
                    break;

                case CommandType.StartSim:
                case CommandType.ResumeSim:
                    State = LayoutState.ListeningCommands;
                    break;

                case CommandType.StopSim:
                    State = LayoutState.Stopped;
                    break;

                case CommandType.PauseSim:
                    State = LayoutState.Paused;
                    break;
            }
            return response;
        }

        private static bool ConvertStringtoBool(string str)
        {
            if (Int32.Parse(str) == 0)
                return false;
            return true;
        }

        private void LogEvent(object? sender, (string? tID, string message) e)
        {
            OnLogEvent?.Invoke(sender, e);
        }
    }
}
