using Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;

namespace LayoutModels.Stations
{
    
    public class Station : BaseStation, ITarget
    {
        // EVENTS
        public event EventHandler<int>? OnPickUp;
        public event EventHandler<(int, Payload)>? OnDropOff;
        public event EventHandler<string>? OnPodReaderPairEvent;
        public event EventHandler<(string, int)>? OnPayloadReaderPairEvent;
        public event EventHandler<string>? OnProcessCompleteEvent;
        public event EventHandler<Payload>? OnPayloadReceived;
        public event EventHandler<Payload>? OnPayloadRelease;
        public event EventHandler<int> OnStopStationAccess;

        private readonly object lockObject = new();

        // VARIABLES
        private Dictionary<int, Payload> _slots = [];

        public Dictionary<int, Payload>  Slots
        {
            get { return _slots; }
            private set 
            { 
                _slots = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SlotCount));
                OnPropertyChanged(nameof(SlotList));

            }
        }
        public int SlotCount => Slots?.Count ?? 0;
        public IEnumerable<SlotWrapper> SlotList
        {
            get
            {
                for (int i = 1; i <= Capacity; i++)
                {
                    yield return new SlotWrapper
                    {
                        SlotIndex = i,
                        Payload = Slots.ContainsKey(i) ? Slots[i] : new Payload("--", string.Empty, string.Empty, 0)
                    };
                }
            }
        }


        public HashSet<int> blockedSlots = [];
        public string PayloadType { get; private set; }
        public Dictionary<string, (string InputState, string OutputState, string NextLocation, float ProcessTime)> PayloadStateMapping { get; private set; }
        public int Capacity { get; private set; }
        public bool Processable { get; private set; }
        public bool PodDockable { get; private set; }
        public bool AutoLoadPod { get; private set; }
        public bool AutoDoorControl { get; private set; }
        public bool PartialProcessApproved { get; set; } = false;
        private long LastActionTime { get; set; }
        public long TimeSinceLastAction
        {
            get
            {
                return internalClock - LastActionTime;
            }
        }

        public string? PodID
        {
            get
            {
                if (PodDockable && podID != null)
                    return podID;
                else
                    throw new ErrorResponse(ErrorCodes.PodNotAvailable, $"Station {StationID} did not have a pod docked.");

            }
            private set
            {
                podID = value;
                Log($"Station {StationID} Pod ID updated to {value}");
            }
        }
        private string? podID = null;
        private int podInputQuantity = 0;
        public bool Mappable { get; private set; }
        public bool StatusMapped
        {
            get { return statusMapped; }
            private set
            {
                statusMapped = value;
                Log($"Station {StationID} Map Status updated to {value}");
            }
        }
        private bool statusMapped = false;

        public bool AllPayloadsSingularInputState
        {
            get
            {
                if (Slots.Keys.Count == 0)
                    return false;
                string payloadState = Slots.Values.First().PayloadState;
                if (!IsAnInputState(payloadState))
                    return false;
                return CheckAllSlots(payloadState);
            }
        }
        public bool AllPayloadsSingularOutputState
        {
            get
            {
                if (Slots.Keys.Count == 0)
                    return false;
                string payloadState = Slots.Values.First().PayloadState;
                if (!IsAnOutputState(payloadState))
                    return false;
                return CheckAllSlots(payloadState);
            }
        }
        public bool AllPayloadsSingularState
        {
            get
            {
                if (Slots.Keys.Count == 0)
                    return false;
                string payloadState = Slots.Values.First().PayloadState;
                return CheckAllSlots(payloadState);
            }
        }
        public bool IsReadyToProcess
        {
            get
            {
                return State == StationState.Idle && Processable && AllPayloadsSingularInputState && (AutoDoorControl || !AutoDoorControl && AllClosableDoorsClosed);
            }
        }
        public bool IsFullAndReadyToProcess
        {
            get
            {
                return IsReadyToProcess && Capacity == Slots.Count;
            }
        }
        public bool IsReadyToUndock
        {
            get
            {
                return State == StationState.Idle && PodDockable && AllPayloadsSingularOutputState && Slots.Keys.Count == podInputQuantity && (AutoDoorControl || !AutoDoorControl && AllClosableDoorsClosed);
            }
        }
        public bool LowPriority { get; set; } = false;


        public List<string> AcceptedCommands { get; private set; }

        // CONSTRUCTORS
        public Station(string stationID, string stationType, string payloadType, Dictionary<string, (string, string, string, float)> payloadStateMap, int capacity,
            List<string> accessibleLocationsWithDoor, List<string> accessibleLocationsWithoutDoor, List<float> doorTransitionTime, bool concurrentLocationAccess,
            bool processable,
            bool podDockable, bool autoLoadPod, bool autoDoorControl,
            bool lowPriority, bool partialProcess,
            List<string> acceptedCommands) : base(stationID, stationType, accessibleLocationsWithDoor, accessibleLocationsWithoutDoor, doorTransitionTime, concurrentLocationAccess)
        {
            State = StationState.Idle;
            PayloadType = payloadType;
            PayloadStateMapping = payloadStateMap;
            Capacity = capacity;
            Processable = processable;
            AutoLoadPod = autoLoadPod || autoDoorControl;
            AutoDoorControl = autoDoorControl;
            PodDockable = podDockable;
            Mappable = podDockable;
            statusMapped = !podDockable;
            LowPriority = lowPriority;
            PartialProcessApproved = partialProcess;
            AcceptedCommands = acceptedCommands;

            if (podDockable) { State = StationState.UnDocked; }
            else { State = StationState.Idle; }

            if (autoDoorControl && !podDockable)
            {
                foreach (string location in Locations.Keys)
                {
                    if (Locations[location].accessLimited)
                        ChangeAccessibility(location, AccessibilityState.Accessible);
                }
            }
        }

        // INTERNAL PROCESSES
        private void Actioned()
        {
            LastActionTime = internalClock;
        }
        public bool CheckInputWafer(string waferState)
        {
            if (!IsAnInputState(waferState))
                return false;
            if (Slots.Count == 0)
                return true;
            if (Slots.Count == Capacity)
                return false;

            foreach (Payload payload in Slots.Values)
            {
                if (IsAnInputState(payload.PayloadState) && payload.PayloadState != waferState)
                    return false;
            }
            return true;
        }

        public bool CheckPayloadCompatible(Payload payload)
        {
            if (payload.PayloadType != PayloadType)
                return false;
            return true;
        }
        public bool CheckSlotEmpty(int slot)
        {
            if (Slots.ContainsKey(slot))
                return false;
            return true;
        }
        private bool CheckAllSlotsEmpty()
        {
            if (Slots.Count == 0) return true;
            return false;
        }
        public int GetNextEmptySlot()
        {
            for (int i = 0; i < Capacity; i++)
            {
                int slot = i + 1;
                if (Slots.ContainsKey(slot) && !blockedSlots.Contains(slot))
                {
                    // Log(new LogMessage($"Station {StationID} Next empty slot ({slot}) was updated."));
                    return slot;
                }
            }
            return Capacity;
        }
        public int GetNextAvailableSlot()
        {
            for (int i = 0; i < Capacity; i++)
            {
                int slot = i + 1;
                if (!Slots.ContainsKey(slot) && !blockedSlots.Contains(slot))
                {
                    // Log(new LogMessage($"Station {StationID} Next available slot ({slot}) was updated."));
                    return slot;
                }
            }
            return -1;
        }
        public List<MapCodes> GetMap()
        {
            List<MapCodes> slotMap = [];

            for (int i = 0; i < Capacity; i++)
            {
                int slot = i + 1;
                if (!Slots.ContainsKey(slot))
                    slotMap.Add(MapCodes.Empty);
                else if (Slots[slot].PayloadErrorStaus)
                    if (slot > 1)
                    {
                        if (slotMap[slot - 1] == MapCodes.Double)
                        {
                            slotMap[slot - 1] = MapCodes.Cross;
                            slotMap.Add(MapCodes.Cross);
                        }
                        else
                            slotMap.Add(MapCodes.Double);
                    }
                    else
                        slotMap.Add(MapCodes.Double);
                else
                    slotMap.Add(MapCodes.Available);
            }

            // Log(new LogMessage($"Station {StationID} map was {slotMap}."));
            return slotMap;
        }

        public List<string> PeakMapData()
        {
            return GetMap().Select(e => ((int)e).ToString()).ToList();
        }
        private bool CheckAllSlots(string payloadState)
        {
            foreach (Payload payload in Slots.Values)
            {
                if (payload.PayloadState != payloadState)
                {
                    return false;
                }
            }
            return true;
        }
        public bool IsAnInputState(string payloadState)
        {
            return PayloadStateMapping.Values.Any(tuple => tuple.InputState == payloadState);
        }
        public bool IsAnOutputState(string payloadState)
        {
            return PayloadStateMapping.Values.Any(tuple => tuple.OutputState == payloadState);
        }

        // COMMANDS
        public void Dock(string tID, Pod pod)
        {
            if (State != StationState.UnDocked)
                throw new ErrorResponse(ErrorCodes.PodAlreadyAvailable, $"Station {StationID} already has pod.");

            if (!CheckAllSlotsEmpty())
                throw new ErrorResponse(ErrorCodes.SlotsNotEmpty, $"Station {StationID} slots are not empty.");

            PodID = pod.PodID;
            Slots = pod.slots;
            podInputQuantity = pod.slots.Values.Count;
            statusMapped = false;

            Log(tID, $"Pod {pod.PodID} was docked to Station {StationID}.");

            if (AutoLoadPod)
            {
                if (Mappable)
                {
                    Log(tID, $"Station {StationID} auto loading and mapping pod {PodID} at {SingleAccessLocation}.");
                    OpenDoorAndMap(tID);
                }
                else
                {
                    if (SingleLocationHasDoor)
                    {
                        Log(tID, $"Station {StationID} auto loading pod {PodID} at {SingleAccessLocation}.");
                        Door(tID, SingleAccessLocation, false);
                    }
                }
            }
            Actioned();

            State = StationState.Idle;
            OnProcessCompleteEvent?.Invoke(this, tID);
        }
        public Pod UnDock(string tID)
        {
            if (State != StationState.Idle)
                throw new ErrorResponse(ErrorCodes.IncorrectState, $"Station {StationID} is in state {State}");
            if (PodID == null)
                throw new ErrorResponse(ErrorCodes.ProgramError, $"Pod ID is missing");

            if (AutoDoorControl)
            {
                CloseAllDoors(tID);
            }
            if (!AllClosableDoorsClosed)
                throw new ErrorResponse(ErrorCodes.IncorrectState, $"Station {StationID} has some open doors");

            Pod pod;
            pod = new(PodID, Capacity, PayloadType)
            {
                slots = Slots
            };
            Slots = [];
            podInputQuantity = 0;
            statusMapped = false;
            State = StationState.UnDocked;
            Actioned();

            Log(tID, $"Pod {pod.PodID} was undocked from Station {StationID}.");
            return pod;
        }
        public void Door(string tID, string location, bool requestedStatus)
        {
            if (!CheckIfDoorExists(location))
            {
                Log(tID, $"Station {StationID} has no door in location {location}.");
                return;
            }

            if (requestedStatus && Locations[location].accessibility == AccessibilityState.Accessible)
            {
                Log(tID, $"Station {StationID} door Closing.");
                State = StationState.Closing;
                ChangeAccessibility(location, AccessibilityState.NotAccessible);
                ProcessWait(tID, Locations[location].transitionTime);
                State = StationState.Idle;
                Log(tID, $"Station {StationID} door Closed.");
            }
            else if (!requestedStatus && Locations[location].accessibility == AccessibilityState.NotAccessible)
            {
                Log(tID, $"Station {StationID} door Opening.");
                State = StationState.Opening;
                ProcessWait(tID, Locations[location].transitionTime);
                ChangeAccessibility(location, AccessibilityState.Accessible);
                State = StationState.Idle;
                Log(tID, $"Station {StationID} door Open.");
            }
            else
            {
                Log(tID, $"Station {StationID} was in state {State}.");
            }
            Actioned();
        }
        private void DoorThread(string tID, string location, bool requestedStatus)
        {
            Door(tID, location, requestedStatus);
        }
        private void CloseAllDoors(string tID)
        {
            List<Thread> threads = [];
            foreach (KeyValuePair<string, (bool accessLimited, AccessibilityState accessibility, float transitionTime)> kvp in Locations)
            {
                if (kvp.Value.accessLimited && kvp.Value.accessibility == AccessibilityState.Accessible)
                {
                    string locationKey = kvp.Key;
                    Thread t = new(() => DoorThread(tID, locationKey, true));
                    t.Start();
                    threads.Add(t);
                }
            }
            while (!AllClosableDoorsClosed || State != StationState.Idle) ;
            Log(tID, $"Station {StationID} All closable doors closed.");
            foreach (Thread thread in threads)
            {
                if (thread.IsAlive)
                {
                    thread.Join();
                }
            }
        }
        private void OpenRelaventDoors(string tID)
        {
            List<Thread> threads = [];
            foreach (KeyValuePair<string, (bool accessLimited, AccessibilityState accessibility, float transitionTime)> kvp in Locations)
            {
                if (kvp.Key == CurrentLocation && kvp.Value.accessLimited && kvp.Value.accessibility != AccessibilityState.Accessible)
                {
                    string locationKey = kvp.Key;
                    Thread t = new(() => DoorThread(tID, locationKey, false));
                    t.Start();
                    threads.Add(t);
                }
            }
            foreach (Thread thread in threads)
            {
                thread.Join();
            }
        }

        public (string? process, float processTime) GetProcessableProcess()
        {
            string? process = null;
            float selectedProcessTime = 0;

            foreach (KeyValuePair<string, (string InputState, string OutputState, string _, float ProcessTime)> kvp in PayloadStateMapping)
            {
                if (kvp.Value.InputState == Slots.Values.First().PayloadState)
                {
                    process = kvp.Key;
                    selectedProcessTime = kvp.Value.ProcessTime;
                }
            }
            return (process, selectedProcessTime);
        }

        public void Process(string tID, string? process = null)
        {
            float selectedProcessTime = 0;
            if (process == null)
            {
                (process, selectedProcessTime) = GetProcessableProcess();
                if (process == null)
                {
                    process = PayloadStateMapping.Keys.First();
                    selectedProcessTime = PayloadStateMapping.Values.First().ProcessTime;
                }
            }

            if (Slots.Count > 0 && PayloadStateMapping[process].InputState == string.Empty)
            {
                throw new ErrorResponse(ErrorCodes.IncorrectState, $"Station {StationID} cannot perform process {process} with payloads");
            }

            //if (CheckAllSlotsEmpty())
            //    throw new ErrorResponse(ErrorCodes.SlotsEmpty, $"Station {StationID} does not have any payloads to process.");
            if (State != StationState.Idle)
                throw new ErrorResponse(ErrorCodes.IncorrectState, $"Station {StationID} is in state {State}");
            if (Slots.Count != 0 && !AllPayloadsSingularInputState)
                throw new ErrorResponse(ErrorCodes.IncorrectState, $"All Payloads in Station {StationID} are not in a singular Input State.");
            if (Slots.Count != 0 && !CheckAllSlots(PayloadStateMapping[process].InputState))
                throw new ErrorResponse(ErrorCodes.IncorrectState, $"All Payloads in Station {StationID} are not in the required input state for process {process} ({PayloadStateMapping[process].InputState}).");

            if (AutoDoorControl && HasDoors)
                CloseAllDoors(tID);

            if (HasDoors && !AllClosableDoorsClosed)
                throw new ErrorResponse(ErrorCodes.IncorrectState, $"Station {StationID} has doors open.");

            State = StationState.Processing;
            Log(tID, $"Station {StationID} Process Started.");

            ProcessWait(tID, selectedProcessTime);

            string mappedState = PayloadStateMapping[process].OutputState;
            CurrentLocation = PayloadStateMapping[process].NextLocation;
            foreach (KeyValuePair<int, Payload> slot in Slots)
            {
                if (mappedState == string.Empty)
                {
                    throw new ErrorResponse(ErrorCodes.IncorrectState, $"Station {StationID} tried to map payloads to impossible states");
                }
                slot.Value.PayloadState = mappedState;
            }

            Actioned();
            State = StationState.Idle;
            Log(tID, $"Station {StationID} Process Complete.");

            if (AutoDoorControl)
                OpenRelaventDoors(tID);

            OnProcessCompleteEvent?.Invoke(this, tID);
        }
        public List<MapCodes> OpenDoorAndMap(string tID, string? location = null)
        {
            if (podID == null)
                throw new ErrorResponse(ErrorCodes.PodNotAvailable, $"Station {StationID} did not have a docked pod.");
            if (State != StationState.Idle && !AutoLoadPod)
                throw new ErrorResponse(ErrorCodes.IncorrectState, $"Station {StationID} is busy.");
            if (!Mappable)
                throw new ErrorResponse(ErrorCodes.IncorrectState, $"Station {StationID} is not mappable.");

            string mapLocation;
            if (location == null)
                mapLocation = Locations.Keys.First();
            else
                mapLocation = location;

            if (AutoDoorControl && Locations[mapLocation].accessLimited && Locations[mapLocation].accessibility == AccessibilityState.Accessible)
                Door(tID, mapLocation, true);

            if (!Locations[mapLocation].accessLimited)
                throw new ErrorResponse(ErrorCodes.IncorrectState, $"Station {StationID} does not have a door");

            State = StationState.Mapping;
            Log(tID, $"Station {StationID} door Mapping.");


            if (!CheckIfDoorExists(mapLocation))
            {
                Log(tID, $"Station {StationID} has no door in location {mapLocation}.");
            }
            else
            {
                ProcessWait(tID, Locations[mapLocation].transitionTime);
            }

            List<MapCodes> slotMap = GetMap();
            ChangeAccessibility(mapLocation, AccessibilityState.Accessible);
            State = StationState.Idle;
            Log(tID, $"Station {StationID} was mapped.");
            return slotMap;
        }

        // MANIPULATOR CONTROLS
        public void StartStationAccess(string location)
        {
            while (State != StationState.Idle) ;
            if (State != StationState.Idle)
                throw new ErrorResponse(ErrorCodes.NotAccessible, $"Station {StationID} busy.");
            if (Locations[location].accessibility != AccessibilityState.Accessible)
                throw new ErrorResponse(ErrorCodes.NotAccessible, $"Station {StationID} door for location {location} is not open.");
            State = StationState.BeingAccessed;
        }
        public void StopStationAccess()
        {
            if (State != StationState.BeingAccessed)
                throw new ErrorResponse(ErrorCodes.ProgramError, $"Station {StationID} should be accessed by robot.");
            State = StationState.Idle;
            OnStopStationAccess?.Invoke(this, 0);
        }
        public string AcceptPayload(string tID, Payload payload, int slot)
        {
            Slots.Add(slot, payload);
            OnPropertyChanged(nameof(SlotCount));
            OnPropertyChanged(nameof(SlotList));
            Log(tID, $"Payload {payload.PayloadID} added to slot {slot} on Station {StationID}.");
            OnDropOff?.Invoke(this, (slot, payload));
            OnPayloadReceived?.Invoke(this, payload);
            UnlockSlot(slot);
            Actioned();
            return payload.PayloadID;
        }
        public Payload ReleasePayload(string tID, int slot)
        {
            Payload payload = Slots[slot];
            Slots.Remove(slot);
            OnPropertyChanged(nameof(SlotCount));
            OnPropertyChanged(nameof(SlotList));
            Log(tID, $"Payload {payload.PayloadID} removed from slot {slot} on Station {StationID}.");
            OnPickUp?.Invoke(this, slot);
            OnPayloadRelease?.Invoke(this, payload);
            UnlockSlot(slot);
            Actioned();
            return payload;
        }

        public void LockSlot(int slot)
        {
            lock (lockObject)
            {
                blockedSlots.Add(slot);
            }
        }

        private void UnlockSlot(int slot)
        {
            if (blockedSlots.Contains(slot))
            {
                lock (lockObject)
                {
                    blockedSlots.Remove(slot);
                }
            }
        }

        public bool IsSlotLocked(int slot)
        {
            return blockedSlots.Contains(slot);
        }

        // READER CONTROLS
        public void PairReader(string readerID, int slot)
        {
            OnPayloadReaderPairEvent?.Invoke(this, (readerID, slot));
            Log($"Station {StationID} was paired with payload reader {readerID} at slot {slot}.");
        }
        public void PairReader(string readerID)
        {
            OnPodReaderPairEvent?.Invoke(this, readerID);
            Log($"Station {StationID} was paired with pod reader {readerID}");
        }
    }
}
