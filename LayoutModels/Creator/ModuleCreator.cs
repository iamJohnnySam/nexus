using LayoutModels.Manipulators;
using LayoutModels.Readers;
using LayoutModels.Stations;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LayoutModels.Creator;

public static class ModuleCreator
{
    private static int GetInt (XElement? val, bool ThrowErrorifNullorBlank = false, string varName = "", int replaceIfNull = 0)
    {
        string? valString = val?.Value;
        if (string.IsNullOrWhiteSpace(valString))
        {
            if (!ThrowErrorifNullorBlank)
                return replaceIfNull;
            else
            {
                throw new ErrorResponse(ErrorCodes.ProgramError, $"Value {varName} was null or empty");
            }
        }
        valString.Trim();
        return int.Parse(valString);
    }
    private static string GetString(XElement? val, bool ThrowErrorIfNullOrBlank = false, string varName = "", string replaceIfNull = "")
    {
        string? valString = val?.Value;
        if (string.IsNullOrWhiteSpace(valString))
        {
            if (!ThrowErrorIfNullOrBlank)
                return replaceIfNull;
            else
            {
                throw new ErrorResponse(ErrorCodes.ProgramError, $"Value {varName} was null or empty");
            }
        }
        return valString.Trim();
    }
    private static bool GetBool(XElement? val, bool ThrowErrorIfNullOrBlank = false, string varName = "", bool replaceIfNull = false)
    {
        string? valString = val?.Value;
        if (string.IsNullOrWhiteSpace(valString))
        {
            if (!ThrowErrorIfNullOrBlank)
                return replaceIfNull;
            else
            {
                throw new ErrorResponse(ErrorCodes.ProgramError, $"Value {varName} was null or empty");
            }
        }
        return valString.Trim() == "1" || valString.Trim().ToLower() == "true";
    }
    private static List<string> GetStringList(XElement? val, bool ThrowErrorIfNullOrBlank = false, string varName = "", string replaceIfNull = "", int ListLength = 0)
    {
        string? valString = val?.Value;
        if (string.IsNullOrWhiteSpace(valString))
        {
            if (!ThrowErrorIfNullOrBlank)
            {
                if (ListLength == 0)
                    return [];
                else
                    return Enumerable.Repeat(replaceIfNull, ListLength).ToList();
            }
                
            else
            {
                throw new ErrorResponse(ErrorCodes.ProgramError, $"Value {varName} was null or empty");
            }
        }
        return valString.Split(',').Select(loc => loc.Trim()).ToList();
    }
    private static List<int> GetIntList(XElement? val, bool ThrowErrorIfNullOrBlank = false, string varName = "", int replaceIfNull = 0, int ListLength = 0)
    {
        string? valString = val?.Value;
        if (string.IsNullOrWhiteSpace(valString))
        {
            if (!ThrowErrorIfNullOrBlank)
            {
                if (ListLength == 0)
                    return [];
                else
                    return Enumerable.Repeat(replaceIfNull, ListLength).ToList();
            }
                
            else
            {
                throw new ErrorResponse(ErrorCodes.ProgramError, $"Value {varName} was null or empty");
            }
        }
        return valString.Split(',').Select(loc => int.Parse(loc.Trim())).ToList();
    }

    public static Dictionary<string, CStation> CreateCStations(IEnumerable<XElement> stations, bool autoMode = false)
    {
        Dictionary<string, CStation> StationList = [];

        foreach (var station in stations)
        {
            string identifier = GetString(station.Element("Identifier"), true, "Identifier");
            int backUpProcessTime = GetInt(station.Element("ProcessTime"));

            List<CProcess> processes = [];
            foreach (var process in station.Descendants("Process"))
            {
                processes.Add(new CProcess()
                {
                    ProcessName = GetString(process.Element("Name")),
                    InputState = GetString(process.Element("InputState")),
                    OutputState = GetString(process.Element("OutputState")),
                    Location = GetString(process.Element("Location"), true, "Location"),
                    ProcessTime = GetInt(process.Element("ProcessTime"), replaceIfNull: backUpProcessTime)
                });
            }

            List<string> doorLocs = GetStringList(station.Element("AccessibleLocationsWithDoor"));
            StationList.Add(identifier, new CStation()
            {
                Identifier = identifier,
                PayloadType = GetString(station.Element("PayloadType"), replaceIfNull: "payload"),
                Capacity = GetInt(station.Element("Capacity"), replaceIfNull: 1),
                Processes = processes,
                AccessibleLocationsWithDoor = doorLocs,
                AccessibleLocationsWithoutDoor = GetStringList(station.Element("AccessibleLocationsWitouthDoor")),
                DoorTransitionTimes = GetIntList(station.Element("DoorTransitionTimes"), replaceIfNull: 0, ListLength: doorLocs.Count),
                ConcurrentLocationAccess = GetBool(station.Element("ConcurrentLocationAccess")),
                Processable = GetBool(station.Element("Processable")),
                PodDockable = GetBool(station.Element("PodDockable")),
                AutoLoadPod = GetBool(station.Element("AutoLoadPod"), replaceIfNull: autoMode),
                AutoDoorControl = GetBool(station.Element("AutoDoorControl"), replaceIfNull: autoMode),
                LowPriority = GetBool(station.Element("LowPriority")),
                PartialProcess = GetBool(station.Element("PartialProcess")),
                Count = GetInt(station.Element("Count")),
                AcceptedCommands = GetStringList(station.Element("AcceptedCommands"))
            });
        }
        return StationList;
    }
    public static Dictionary<string, Station> CreateStations(IEnumerable<XElement> stations, bool autoMode)
    {
        Dictionary<string, Station> StationList = [];
        Dictionary<string, CStation> CStations = CreateCStations(stations, autoMode);

        foreach (KeyValuePair<string, CStation> kvp in CStations)
        {
            Dictionary<string, (string, string, string, float)> map = [];
            foreach (CProcess process in kvp.Value.Processes)
            {
                map.Add(process.ProcessName, (process.InputState, process.OutputState, process.Location, process.ProcessTime));
            }

            CStation cStation = kvp.Value;
            for (int i = 0; i < cStation.Count; i++)
            {
                int j = i + 1;
                string stationName = $"{cStation.Identifier}{j++}";
                while (StationList.ContainsKey(stationName))
                    stationName = $"{cStation.Identifier}{j}";

                StationList.Add(stationName, new Station(
                    stationID: stationName,
                    stationType: cStation.Identifier,
                    payloadType: cStation.PayloadType,
                    payloadStateMap: map,
                    capacity: cStation.Capacity,
                    accessibleLocationsWithDoor: cStation.AccessibleLocationsWithDoor,
                    accessibleLocationsWithoutDoor: cStation.AccessibleLocationsWithoutDoor,
                    doorTransitionTime: cStation.DoorTransitionTimes.Select(i => (float)i).ToList(),
                    concurrentLocationAccess: cStation.ConcurrentLocationAccess,
                    processable: cStation.Processable,
                    podDockable: cStation.PodDockable,
                    autoLoadPod: cStation.AutoLoadPod,
                    autoDoorControl: cStation.AutoDoorControl,
                    lowPriority: cStation.LowPriority,
                    partialProcess: cStation.PartialProcess,
                    acceptedCommands: cStation.AcceptedCommands
                    ));
                StationList[stationName].Tickable = !autoMode;
            }
        }
        return StationList;
    }

    public static Dictionary<string, CManipulator> CreateCManipulators(IEnumerable<XElement> manipulators, bool autoMode = false)
    {
        Dictionary<string, CManipulator> ManipulatorList = [];
        foreach (var manipulator in manipulators)
        {
            string identifier = GetString(manipulator.Element("Identifier"), true, "Identifier");
            ManipulatorList.Add(identifier, new CManipulator()
            {
                Identifier = identifier,
                EndEffectors = GetStringList(manipulator.Element("EndEffectors"), replaceIfNull: "payload", ListLength: 1),
                Locations = GetStringList(manipulator.Element("Locations"), replaceIfNull: "location"),
                MotionTime = GetInt(manipulator.Element("MotionTime"), replaceIfNull: 0),
                ExtendTime = GetInt(manipulator.Element("ExtendTime"), replaceIfNull: 0),
                RetractTime = GetInt(manipulator.Element("RetractTime"), replaceIfNull: 0),
                Count = GetInt(manipulator.Element("Count"), replaceIfNull: 0)
            });
        }
        return ManipulatorList;
    }
    public static Dictionary<string, Manipulator> CreateManipulators(IEnumerable<XElement> manipulators, bool autoMode)
    {
        Dictionary<string, Manipulator> ManipulatorList = [];
        Dictionary<string, CManipulator> CManipulators = CreateCManipulators(manipulators, autoMode);

        foreach (KeyValuePair<string, CManipulator> kvp in CManipulators)
        {
            CManipulator cManipulator = kvp.Value;

            List<string> endEffectorsTypes = cManipulator.EndEffectors;
            if (endEffectorsTypes.Count == 0)
                endEffectorsTypes.Add("payload");

            Dictionary<int, Dictionary<string, Payload>> endEffectors = [];
            int endEffector = 1;
            foreach (string payload in endEffectorsTypes)
                endEffectors.Add(endEffector++, []);

            for (int i = 0; i < cManipulator.Count; i++)
            {
                int j = i + 1;
                string manipulatorName = $"{cManipulator.Identifier}{j++}";
                while (ManipulatorList.ContainsKey(manipulatorName))
                    manipulatorName = $"{cManipulator.Identifier}{j}";

                ManipulatorList.Add(manipulatorName, new Manipulator(
                    manipulatorName, cManipulator.Identifier,
                    endEffectors, endEffectorsTypes, 
                    cManipulator.Locations, 
                    cManipulator.MotionTime, 
                    cManipulator.ExtendTime, 
                    cManipulator.RetractTime));
                ManipulatorList[manipulatorName].Tickable = !autoMode;
            }
        }
        return ManipulatorList;
    }

    public static Dictionary<string, CReader> CreateCReaders(IEnumerable<XElement> readers)
    {
        Dictionary<string, CReader> Readers = [];
        foreach (var reader in readers)
        {
            string identifier = GetString(reader.Element("Identifier"), true, "Identifier");
            Readers.Add(identifier, new CReader()
            {
                Identifier = identifier,
                StationID = GetString(reader.Element("StationIdentifier"), true, "StationIdentifier"),
                Slot = GetInt(reader.Element("Slot"), replaceIfNull: 0)
            });
        }
        return Readers;
    }
    public static Dictionary<string, Reader> CreateReaders(IEnumerable<XElement> readers, Dictionary<string, Station> stationList)
    {
        Dictionary<string, Reader> Readers = [];
        Dictionary<string, CReader> CReaders = CreateCReaders(readers);

        foreach (KeyValuePair<string, CReader> kvp in CReaders)
        {
            CReader cReader = kvp.Value;
            string readerName = cReader.Identifier;
            int slot = cReader.Slot;

            int j = 1;
            string targetStation = $"{cReader.StationID}{j}";

            if (!stationList.ContainsKey(targetStation))
                throw new ErrorResponse(ErrorCodes.ProgramError, $"Station {cReader.StationID} not found for reader {readerName}");

            while (stationList.ContainsKey(targetStation))
            {
                if (slot <= 0)
                    Readers.Add($"{cReader.Identifier}{j++}", new Reader(readerName, stationList[cReader.StationID].PayloadType, stationList[cReader.StationID]));
                else
                    Readers.Add($"{cReader.Identifier}{j++}", new Reader(readerName, stationList[cReader.StationID].PayloadType, stationList[cReader.StationID], slot));
                targetStation = $"{cReader.StationID}{j}";
            }

        }
        return Readers;
    }
}
