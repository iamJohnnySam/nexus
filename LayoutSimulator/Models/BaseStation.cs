using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutSimulator.Models;

public class BaseStation
{
    public event EventHandler<LogMessage>? OnLogEvent;
    public event EventHandler<EStationState>? OnStateChangeEvent;

    public EStationState State
    {
        get { return state; }
        internal set
        {
            if (state != value)
            {
                state = value;
                Log(new LogMessage($"Station {StationID} State was updated to {value}"));
                OnStateChangeEvent?.Invoke(this, value);
            }
        }
    }
    private EStationState state;
    public string StationType { get; set; }
    public bool ConcurrentLocationAccess { get; private set; } = true;
    public string CurrentLocation { get; internal set; }
    public string StartLocation { get; internal set; }
    public bool SingleLocationAccess
    {
        get
        {
            return Locations.Keys.Count == 1;
        }
    }
    public bool SingleLocationHasDoor
    {
        get
        {
            return SingleLocationAccess && Locations.First().Value.accessLimited;
        }
    }
    public string SingleAccessLocation
    {
        get
        {
            return Locations.First().Key;
        }
    }
    public bool HasDoors { get; } = false;
    public bool AllClosableDoorsClosed
    {
        get
        {
            foreach ((bool hasDoor, EAccessibilityState accessibility, float _) in Locations.Values)
            {
                if (hasDoor && accessibility != EAccessibilityState.NotAccessible)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public string StationID { get; set; }

    public Dictionary<string, (bool accessLimited, EAccessibilityState accessibility, float transitionTime)> Locations = [];
    public bool Tickable { get; set; } = false;

    public long internalClock = 0;
    public long showProcessTime = 0;
    public long showTime = 0;

    public BaseStation(string stationID, string stationType, List<string> accessibleLocationsWithDoors, List<string> accessibleLocationsWithoutDoors, List<float> doorTransitionTime, bool concurrentLocationAccess)
    {
        if (accessibleLocationsWithDoors.Count == 1 && accessibleLocationsWithDoors[0] == "")
        {
            accessibleLocationsWithDoors = [];
        }
        if (accessibleLocationsWithoutDoors.Count == 1 && accessibleLocationsWithoutDoors[0] == "")
        {
            accessibleLocationsWithoutDoors = [];
        }
        if (doorTransitionTime.Count == 1 && doorTransitionTime[0] == 0)
        {
            doorTransitionTime = [];
        }


        if (accessibleLocationsWithDoors.Count == 0 && accessibleLocationsWithoutDoors.Count == 0)
            throw new ErrorResponse(EErrorCode.ProgramError, $"No locations for Station {StationID}.");

        StationID = stationID;
        StationType = stationType;
        ConcurrentLocationAccess = concurrentLocationAccess;

        if (accessibleLocationsWithDoors.Count > 0)
            HasDoors = true;

        if (accessibleLocationsWithDoors.Count == 0)
        {
            CurrentLocation = accessibleLocationsWithoutDoors[0];
            StartLocation = CurrentLocation;
        }

        else
        {
            CurrentLocation = accessibleLocationsWithDoors[0];
            StartLocation = CurrentLocation;
        }


        int i = 0;
        if (accessibleLocationsWithDoors.Count != doorTransitionTime.Count)
            throw new ErrorResponse(EErrorCode.ProgramError, $"Door Count ({accessibleLocationsWithDoors.Count}) and door transisition time count ({accessibleLocationsWithoutDoors.Count}) did not match");

        foreach (string location in accessibleLocationsWithDoors)
        {
            Locations.Add(location, (true, EAccessibilityState.NotAccessible, doorTransitionTime[i]));
            i++;
        }
        foreach (string location in accessibleLocationsWithoutDoors)
        {
            if (location == "")
            {
                continue;
            }
            if (Locations.ContainsKey(location))
                throw new ErrorResponse(EErrorCode.ProgramError, $"Location {location} repeated in both lists");
            else
                Locations.Add(location, (false, EAccessibilityState.Accessible, 0));
        }
        Log(new LogMessage($"{StationID} Created"));
    }
    public BaseStation(string stationID, string stationType, List<string> locations)
    {
        StationID = stationID;
        StationType = stationType;
        CurrentLocation = "home";
        StartLocation = string.Empty;
        foreach (string location in locations)
        {
            Locations.Add(location, (false, EAccessibilityState.Accessible, 0));
        }
        Log(new LogMessage($"{StationID} Created"));
    }
    public BaseStation(string stationID, string stationType)
    {
        StationID = stationID;
        StationType = stationType;
        StartLocation = string.Empty;
        CurrentLocation = string.Empty;
        Log(new LogMessage($"{StationID} Created"));
    }

    public bool CheckIfDoorExists(string location)
    {
        if (!Locations.ContainsKey(location))
            throw new ErrorResponse(EErrorCode.ProgramError, $"Station {StationID} does not have location {location}.");
        return Locations[location].accessLimited;
    }
    public bool CheckAccessible(string location)
    {
        if (Locations.TryGetValue(location, out (bool accessLimited, EAccessibilityState accessibility, float transitionTime) value) && value.accessibility == EAccessibilityState.Accessible)
        {
            return true;
        }
        return false;
    }
    internal void ChangeAccessibility(string location, EAccessibilityState accessibility)
    {
        if (Locations.ContainsKey(location))
            Locations[location] = (Locations[location].accessLimited, accessibility, Locations[location].transitionTime);
        else
            throw new ErrorResponse(EErrorCode.ProgramError, $"Unknown location {location} in {Locations.Keys}");
    }

    public void ProcessWait(string tID, float SecsTime)
    {
        if (Tickable)
        {
            OnLogEvent?.Invoke(this, new LogMessage(tID, $"{StationID} Processing"));
            long startTime = internalClock;
            while (SecsTime >= (internalClock - startTime))
            {
                // todo: use events and get rid of while loop
                showProcessTime = (long)SecsTime;
                if (showTime < (internalClock - startTime))
                    showTime = internalClock - startTime;
                Thread.Sleep(1);
            }
            // OnLogEvent?.Invoke(this, new LogMessage(tID, $"{StationID} Done Processing"));
            showTime = 0;
            showProcessTime = 0;
        }
        else
        {
            Thread.Sleep((int)(SecsTime * 1000));
        }
    }
    public void Tick()
    {
        internalClock++;
    }

    protected virtual void Log(LogMessage message)
    {
        OnLogEvent?.Invoke(this, message);
    }

}
