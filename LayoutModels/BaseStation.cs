﻿using LayoutModels.Stations;
using Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UIUtilities;

namespace LayoutModels
{
    public enum AccessibilityState
    {
        Accessible,
        NotAccessible
    }
    public class BaseStation : ViewModelBase
    {
        public event EventHandler<(string? tID, string message)>? OnLogEvent;
        public event EventHandler<StationState>? OnStateChangeEvent;

        public StationState State
        {
            get { return state; }
            internal set
            {
                if (state != value)
                {
                    state = value;
                    Log($"Station {StationID} State was updated to {value}");
                    OnStateChangeEvent?.Invoke(this, value);
                    OnPropertyChanged();
                }
            }
        }
        private StationState state;
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
                foreach ((bool hasDoor, AccessibilityState accessibility, float _) in Locations.Values)
                {
                    if (hasDoor && accessibility != AccessibilityState.NotAccessible)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public string StationID { get; set; }

        public Dictionary<string, (bool accessLimited, AccessibilityState accessibility, float transitionTime)> Locations = [];
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
                throw new ErrorResponse(ErrorCodes.ProgramError, $"No locations for Station {StationID}.");

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
                throw new ErrorResponse(ErrorCodes.ProgramError, $"Door Count ({accessibleLocationsWithDoors.Count}) and door transisition time count ({accessibleLocationsWithoutDoors.Count}) did not match");

            foreach(string location in accessibleLocationsWithDoors)
            {
                Locations.Add(location, (true, AccessibilityState.NotAccessible, doorTransitionTime[i]));
                i++;
            }
            foreach (string location in accessibleLocationsWithoutDoors)
            {
                if (location == "")
                {
                    continue;
                }
                if (Locations.ContainsKey(location))
                    throw new ErrorResponse(ErrorCodes.ProgramError, $"Location {location} repeated in both lists");
                else
                    Locations.Add(location, (false, AccessibilityState.Accessible, 0));
            }
            Log($"{StationID} Created");
        }
        public BaseStation(string stationID, string stationType, List<string> locations)
        {
            StationID = stationID;
            StationType = stationType;
            CurrentLocation = "home";
            StartLocation = string.Empty;
            foreach (string location in locations)
            {
                Locations.Add(location, (false, AccessibilityState.Accessible, 0));
            }
            Log($"{StationID} Created");
        }
        public BaseStation(string stationID, string stationType)
        {
            StationID = stationID;
            StationType = stationType;
            StartLocation = string.Empty;
            CurrentLocation = string.Empty;
            Log($"{StationID} Created");
        }

        public bool CheckIfDoorExists(string location)
        {
            if (!Locations.TryGetValue(location, out (bool accessLimited, AccessibilityState accessibility, float transitionTime) value))
                throw new ErrorResponse(ErrorCodes.ProgramError, $"Station {StationID} does not have location {location}.");
            return value.accessLimited;
        }
        public bool CheckAccessible(string location)
        {
            if (Locations.TryGetValue(location, out (bool accessLimited, AccessibilityState accessibility, float transitionTime) value) && value.accessibility == AccessibilityState.Accessible)
            {
                return true;
            }
            return false;
        }
        internal void ChangeAccessibility(string location, AccessibilityState accessibility)
        {
            if (Locations.ContainsKey(location))
                Locations[location] = (Locations[location].accessLimited, accessibility, Locations[location].transitionTime);
            else
                throw new ErrorResponse(ErrorCodes.ProgramError, $"Unknown location {location} in {Locations.Keys}");
        }

        public void ProcessWait(string tID, float SecsTime)
        {
            if (Tickable)
            {
                Log(tID, $"{StationID} Processing");
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

        protected virtual void Log (string message)
        {
            OnLogEvent?.Invoke(this, (null, message));
        }
        protected virtual void Log(string tID, string message)
        {
            OnLogEvent?.Invoke(this, (tID, message));
        }

    }
}
