using LayoutModels;
using LayoutModels.Manipulators;
using LayoutModels.Stations;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SequenceSimulator
{
    public class Simulator
    {
        private readonly object lockObject = new();

        public bool IgnoreLotIDMatching { get; set; }

        public event EventHandler<(string? tID, string message)>? OnLogEvent;
        int transactionID = 0;
        public int completedPayloads = 0;

        public int TotalTime { get; set; }
        public float Throughput { get; private set; }
        public float SteadyStateThroughput { get; private set; }

        public readonly Layout layout;

        public List<(string PayloadID, string StationID, int Slot, bool Priority)> waitingTransfer = [];
        public HashSet<string> blockedPayloads = [];
        public HashSet<Manipulator> blockedManipulators = [];
        public HashSet<Station> blockedStations = [];

        public List<string> stationsNotFilled = [];
        public float steadyStateTime = 0;

        long maxManipulatorTime = 0;

        public float CalculateSteadyStateThroughput 
        { 
            get
            {
                return completedPayloads * 3600 / ((float)TotalTime - steadyStateTime);
            } 
        }
        public float CalculateThroughput
        {
            get
            {
                return completedPayloads * 3600 / (float)TotalTime;
            }
        }

        public Simulator()
        {
            layout = new Layout();
            layout.OnLogEvent += LogEvent;
        }

        public void InitializeSimulator(string xmlPath, bool ignoreLotIDMatching)
        {
            IgnoreLotIDMatching = ignoreLotIDMatching;

            layout.InitializeLayout(xmlPath, false);
            foreach (Manipulator manipulator in layout.ManipulatorList.Values)
            {
                manipulator.OnPickUp += Manipulator_OnPickUp;
                long manipulatorTotalTime = (long)(manipulator.ExtendTime + manipulator.RetractTime + manipulator.MotionTime);
                if (maxManipulatorTime < manipulatorTotalTime)
                {
                    maxManipulatorTime = manipulatorTotalTime;
                }
            }
            foreach(Station station in layout.StationList.Values)
            {
                station.OnProcessCompleteEvent += Station_OnProcessCompleteEvent;
                station.OnPayloadReceived += Station_OnPayloadAction;
                station.OnPayloadRelease += Station_OnPayloadAction;

                if (station.PodDockable)
                {
                    station.OnPayloadReceived += PayloadComplete;
                }
                if (station.Processable && !station.LowPriority && !station.PartialProcessApproved)
                {
                    station.OnPayloadReceived += PayloadReceived;
                    if (!stationsNotFilled.Contains(station.StationID))
                    {
                        stationsNotFilled.Add(station.StationID);
                    }
                }
            }
        }

        // Payload
        private void PayloadComplete(object? sender, Payload e)
        {
            completedPayloads++;
            Throughput = CalculateThroughput;

            if (stationsNotFilled.Count == 0)
            {
                if (steadyStateTime == 0)
                    steadyStateTime = (float)TotalTime;
                SteadyStateThroughput = CalculateSteadyStateThroughput;
                return;
            }
        }

        private void PayloadReceived(object? sender, Payload e)
        {
            if (sender != null)
            {
                Station station = (Station)sender;
                if ((stationsNotFilled.Contains(station.StationID)) && station.slots.Count == station.Capacity)
                {
                    stationsNotFilled.Remove(station.StationID);
                    if (stationsNotFilled.Count == 0)
                    {
                        steadyStateTime = (float)TotalTime;
                    }
                }
            }
        }

        // Remove from block lists
        private void Station_OnProcessCompleteEvent(object? sender, string e)
        {
            if (sender is Station station)
            {
                lock (lockObject)
                {
                    blockedStations.Remove(station);
                }
            }
        }

        private void Station_OnPayloadAction(object? sender, Payload e)
        {
            if (sender is Station station)
            {
                lock (lockObject)
                {
                    blockedStations.Remove(station);
                }
            }  
        }

        private void Manipulator_OnPickUp(object? sender, (int, string, Payload) e)
        {
            if (blockedPayloads.Contains(e.Item3.PayloadID))
            {
                lock (lockObject)
                {
                    blockedPayloads.Remove(e.Item3.PayloadID);
                }
            }
        }

        // Simulator Execution
        public void RunSimulator(int timeInUnits)
        {
            for (int time = 0; time < timeInUnits; time++)
            {
                RunSimulatorForSingleUnit();
                Thread.Sleep(10);
            }
        }
        private void RunSimulatorForSingleUnit()
        {
            removeThreads();
            TotalTime++;
            //OnLogEvent?.Invoke(this, new LogMessage($"Time = {TotalTime}"));
            CheckStations();
            CheckTransfers(true);
            CheckTransfers(false);
            CheckManipulators();
        }

        // Station Execution
        private void CheckStations()
        {
            foreach (string stationID in layout.StationList.Keys)
            {
                Station station = layout.StationList[stationID];
                station.Tick();

                // Tick if busy
                if (station.State != StationState.Idle && blockedStations.Contains(station))
                {
                    continue;
                }

                if (blockedStations.Contains(station))
                {
                    continue;
                }

                // Check Dock-able
                if (station.State == StationState.UnDocked)
                {
                    lock (lockObject)
                    {
                        blockedStations.Add(station);
                    }
                    Thread t = new(() => CreatePodAndDock(transactionID++.ToString(), stationID));
                    t.Start();
                    Global.RunningThreads.Add(t);
                    continue;
                }

                // Check if station is processable with partial wafers
                bool runableAtPartial = false;
                if (station.AllPayloadsSingularInputState && station.PartialProcessApproved && station.TimeSinceLastAction > (3 * maxManipulatorTime) && station.IsReadyToProcess)
                {
                    (string? process, float pTime) = station.GetProcessableProcess();
                    if (process != null && station.TimeSinceLastAction > pTime)
                    {
                        runableAtPartial = true;
                    }
                }

                bool runNoMoreWafers = false;
                if (TotalTime > 3 * maxManipulatorTime && waitingTransfer.Count == 0 && station.AllPayloadsSingularInputState && station.IsReadyToProcess)
                    runNoMoreWafers = true;


                // Check Processable
                if ((station.IsFullAndReadyToProcess || runableAtPartial || runNoMoreWafers) && station.State == StationState.Idle && !IsAnyRobotEnRouteToStation(station.StationID))
                {
                    lock (lockObject)
                    {
                        blockedStations.Add(station);
                    }
                    DropStationFromWaitingList(station.StationID);
                    Thread t = new(() => layout.ProcessStation(transactionID++.ToString(), stationID));
                    t.Start();
                    Global.RunningThreads.Add(t);
                    continue;
                }

                // 2025.03.08 Send Station back to Home if no wafers
                if ((station.State == StationState.Idle) && station.Processable && (station.slots.Values.Count == 0) && (station.CurrentLocation != station.StartLocation) && !IsAnyRobotEnRouteToStation(station.StationID))
                {
                    lock (lockObject)
                    {
                        blockedStations.Add(station);
                    }
                    string process = station.PayloadStateMapping.FirstOrDefault(x => x.Value.NextLocation == station.StartLocation).Key;
                    Thread t = new(() => layout.ProcessStation(transactionID++.ToString(), stationID, process));
                    t.Start();
                    Global.RunningThreads.Add(t);
                    continue;
                }

                // Check Undock-able
                if (station.IsReadyToUndock && station.State == StationState.Idle && !IsAnyRobotEnRouteToStation(station.StationID))
                {
                    lock (lockObject)
                    {
                        blockedStations.Add(station);
                    }
                    DropStationFromWaitingList(station.StationID);
                    Thread t = new(() => SwapPod(transactionID++.ToString(), stationID));
                    t.Start();
                    Global.RunningThreads.Add(t);
                    continue;
                }

                // sort the list according to priority
                waitingTransfer = waitingTransfer.OrderByDescending(item => item.Priority).ToList();

                foreach (int slot in station.slots.Keys.ToList())
                {
                    Payload payload = station.slots[slot];

                    if (((station.Processable && station.IsAnOutputState(payload.PayloadState)) || (station.Processable && station.LowPriority && station.SingleLocationAccess) || (!station.Processable && station.IsAnInputState(payload.PayloadState)) || (station.Processable && !station.AllPayloadsSingularState)) && station.State == StationState.Idle && !blockedStations.Contains(station))
                    {
                        if (!waitingTransfer.Any(item => item.PayloadID == payload.PayloadID) && !blockedPayloads.Contains(payload.PayloadID))
                        {
                            waitingTransfer.Add((payload.PayloadID, stationID, slot, !station.LowPriority));
                            OnLogEvent?.Invoke(this, (null, $"Payload {payload.PayloadID} added to waiting list"));
                        }
                    }
                }

                // sometimes the simulation doesn't unblock the systems. This is a fail safe
                if (station.State == StationState.Idle && blockedStations.Contains(station) && !IsAnyRobotEnRouteToStation(stationID))
                {
                    blockedStations.Remove(station);
                }
            }
        }

        private void DropStationFromWaitingList(string stationID)
        {
            foreach ((string, string, int, bool) transferItem in waitingTransfer.ToList())
            {
                if (transferItem.Item2 == stationID)
                {
                    waitingTransfer.Remove(transferItem);
                }
            }
        }
        private void SwapPod(string tID, string stationID)
        {
            layout.UndockPod(tID, stationID);
            lock (lockObject)
            {
                blockedStations.Remove(layout.StationList[stationID]);
            }
        }


        private void CheckTransfers(bool swap = false)
        {
            foreach((string thisPayloadID, string thisStationID, int swapSlot, bool _) in waitingTransfer.ToList())
            {
                // Attempt Swap: Check needed wafer
                Station thisStation = layout.StationList[thisStationID];

                if (!thisStation.slots.ContainsKey(swapSlot))
                {
                    continue;
                }
                if (thisStation.IsSlotLocked(swapSlot) || blockedStations.Contains(thisStation))
                {
                    continue;
                }
                Payload thisPayload = thisStation.slots[swapSlot];

                // Check if there is an output location
                List<string> thisStationAccessibleLocations;
                if (thisStation.ConcurrentLocationAccess)
                    thisStationAccessibleLocations = [.. thisStation.Locations.Keys];
                else
                    thisStationAccessibleLocations = [thisStation.CurrentLocation];
                (Station? swapOutStation, List<string> swapOutLocations) = CheckIfOutputStationAvailable(thisPayload, thisStation);
                if (swapOutStation == null)
                    continue;


                (swapOutStation, int toSlot) = GetTargetStationAndSlot(swapOutStation, thisPayload);
                if (toSlot < 0)
                {
                    continue;
                }

                // Check if there is a swap in station
                string? swapInPayloadID = CheckIfSwappablePayloadExists(thisStation, swapOutLocations);
                
                // Initiate motion
                if (swapInPayloadID !=null && swapOutStation!=null && !thisStation.PodDockable)
                {
                    Station swapInStation = layout.StationList[waitingTransfer.FirstOrDefault(item => item.PayloadID == swapInPayloadID).StationID];
                    int swapInSlot = waitingTransfer.FirstOrDefault(item => item.PayloadID == swapInPayloadID).Slot;
                    InitiateSwap(swapInPayloadID, thisPayloadID, swapInStation, swapInSlot, thisStation, swapSlot, swapOutStation, toSlot);
                }
                else if (swapOutStation != null && !swap)
                {
                    InitiateTransfer(thisPayloadID, thisStation, swapSlot, swapOutStation, toSlot);
                }
            }
        }
        private string? CheckIfSwappablePayloadExists(Station station, List<string>accessibleLocations)
        {
            string? returnPayloadID = null;
            foreach ((string swapInPayloadID, string swapInStationID, int swapInSlot, bool _) in waitingTransfer)
            {

                if (station.StationID == swapInStationID){
                    continue;
                }

                Station swapInStation = layout.StationList[swapInStationID];

                if (!swapInStation.slots.ContainsKey(swapInSlot) || swapInStation.IsSlotLocked(swapInSlot) || blockedStations.Contains(swapInStation))
                {
                    continue;
                }

                if (station.IsAnInputState(swapInStation.slots[swapInSlot].PayloadState))
                {
                    if (!swapInStation.ConcurrentLocationAccess && accessibleLocations.Contains(swapInStation.CurrentLocation))
                        return swapInPayloadID;

                    else if (swapInStation.ConcurrentLocationAccess && HaveCommonElement([accessibleLocations, [.. swapInStation.Locations.Keys]]))
                        return swapInPayloadID;

                    else if (!swapInStation.ConcurrentLocationAccess && !station.ConcurrentLocationAccess && station.CurrentLocation == swapInStation.CurrentLocation)
                        return swapInPayloadID;

                    else if (!swapInStation.ConcurrentLocationAccess && station.ConcurrentLocationAccess && station.Locations.ContainsKey(swapInStation.CurrentLocation))
                        return swapInPayloadID;

                    else if (swapInStation.ConcurrentLocationAccess && !station.ConcurrentLocationAccess && swapInStation.Locations.ContainsKey(station.CurrentLocation))
                        return swapInPayloadID;

                    else if (swapInStation.ConcurrentLocationAccess && station.ConcurrentLocationAccess && HaveCommonElement([[.. station.Locations.Keys], [.. swapInStation.Locations.Keys]]))
                        return swapInPayloadID;
                    // todo:
                    returnPayloadID = swapInPayloadID;
                }
            }
            return returnPayloadID;
        }
        private (Station? station, List<string> swapOutLocations) CheckIfOutputStationAvailable(Payload payload, Station currentStation)
        {
            Station? lowPriorityStation = null;
            List<string> lowPriorityStationLocations = [];

            Station? nonProcessableStation = null;
            List<string> nonProcessableStationLocations = [];

            foreach (Station nextStation in layout.StationList.Values)
            {

                if (nextStation.State != StationState.Idle || blockedStations.Contains(nextStation) || nextStation.StationID == currentStation.StationID || IsAnyRobotEnRouteToStation(nextStation.StationID))
                {
                    continue;
                }

                // If the wafer is currently at a non-processable station. Don't move it to another non-processable / non-pod-dockable station which only has access to one location.
                if (!currentStation.Processable && (!nextStation.Processable && nextStation.SingleLocationAccess && !nextStation.PodDockable))
                {
                    continue;
                }

                // next station is the same station
                if (currentStation.StationType == nextStation.StationType)
                {
                    continue;
                }

                // station is full
                if (nextStation.slots.Count == nextStation.Capacity)
                {
                    continue;
                }

                if (((!nextStation.PodDockable && nextStation.CheckInputWafer(payload.PayloadState)) || (nextStation.PodDockable && nextStation.IsAnOutputState(payload.PayloadState))))
                {
                    if (nextStation.ConcurrentLocationAccess && HaveCommonElement([nextStation.Locations.Keys, currentStation.Locations.Keys]))
                    {
                        List<string>? commonLocations = GetCommonElements([nextStation.Locations.Keys, currentStation.Locations.Keys]);
                        if (commonLocations != null)
                        {
                            if (nextStation.Processable && !nextStation.LowPriority)
                            {
                                return (nextStation, commonLocations);
                            }
                            else if (nextStation.LowPriority)
                            {
                                lowPriorityStation = nextStation;
                                lowPriorityStationLocations = commonLocations;
                            }
                            else
                            {
                                nonProcessableStation = nextStation;
                                nonProcessableStationLocations = commonLocations;
                            }     
                        }
                        else
                            throw new ErrorResponse(ErrorCodes.ProgramError, "Unknown situation.");
                    }
                    else if (!nextStation.ConcurrentLocationAccess && currentStation.Locations.Keys.Contains(nextStation.CurrentLocation))
                    {
                        if (nextStation.Processable && !nextStation.LowPriority)
                        {
                            return (nextStation, [nextStation.CurrentLocation]);
                        }
                        else if (nextStation.LowPriority)
                        {
                            lowPriorityStation = nextStation;
                            lowPriorityStationLocations = [nextStation.CurrentLocation];
                        }
                        else
                        {
                            nonProcessableStation = nextStation;
                            nonProcessableStationLocations = [nextStation.CurrentLocation];
                        }
                    }
                    else
                    {
                        lowPriorityStation = nextStation;
                    }
                }
            }

            if (nonProcessableStation != null)
            {
                return (nonProcessableStation, nonProcessableStationLocations);
            }
            else
            {
                return (lowPriorityStation, lowPriorityStationLocations);
            }
        }

        private void InitiateTransfer( string payloadID, Station fromStation, int fromSlot, Station toStation, int toSlot)
        {
            Manipulator? manipulator = CheckAvailableAccessibleManipulators(fromStation, toStation);
            if (manipulator != null)
            {
                lock (lockObject)
                {
                    blockedManipulators.Add(manipulator);
                    blockedStations.Add(toStation);
                    blockedStations.Add(fromStation);
                }
                fromStation.LockSlot(fromSlot);
                toStation.LockSlot(toSlot);
                Thread t = new(() => RunTransfer(transactionID++.ToString(), manipulator, fromStation, fromSlot, toStation, toSlot));
                t.Start();
                Global.RunningThreads.Add(t);
                waitingTransfer.Remove(waitingTransfer.FirstOrDefault(item => item.PayloadID == payloadID));
                lock (lockObject)
                {
                    blockedPayloads.Add(payloadID);
                }
            }
        }
        private void InitiateSwap(string swapInPayloadID, string swapOutPayloadID, Station pickStation, int pickSlot, Station swapStation, int swapSlot, Station putStation, int putSlot)
        {
            // todo: if manipulator cannot access all 3 stations for swap then initiate transfer

            Manipulator? manipulator = CheckAvailableAccessibleManipulators(pickStation, swapStation, putStation);
            if (manipulator == null)
            {
                InitiateTransfer(swapOutPayloadID, swapStation, swapSlot, putStation, putSlot);
            }

            if (manipulator != null && (manipulator.EndEffectorTypes.Count > 1) && HasDuplicate(manipulator.EndEffectorTypes))
            {
                lock (lockObject)
                {
                    blockedManipulators.Add(manipulator);
                    blockedStations.Add(pickStation);
                    blockedStations.Add(swapStation);
                    blockedStations.Add(putStation);
                }
                pickStation.LockSlot(pickSlot);
                swapStation.LockSlot(swapSlot);
                putStation.LockSlot(putSlot);
                waitingTransfer.Remove(waitingTransfer.FirstOrDefault(item => item.PayloadID == swapInPayloadID));
                lock (lockObject)
                {
                    blockedPayloads.Add(swapInPayloadID);
                }
                waitingTransfer.Remove(waitingTransfer.FirstOrDefault(item => item.PayloadID == swapOutPayloadID));
                lock (lockObject)
                {
                    blockedPayloads.Add(swapOutPayloadID);
                }
                Thread t = new(() => RunSwap(transactionID++.ToString(), manipulator, pickStation, pickSlot, swapStation, swapSlot, putStation, putSlot));
                t.Start();
                Global.RunningThreads.Add(t);
            }
        }

        private int GetSuitableEE(Manipulator manipulator, Station station)
        {
            return manipulator.EndEffectorTypes.IndexOf(station.PayloadType);
        }

        private void RunTransfer(string tID, Manipulator manipulator, Station fromStation, int fromSlot, Station toStation, int toSlot)
        {
            OnLogEvent?.Invoke(this, (tID, $"Transfer Initiated for {manipulator.StationID} from {fromStation.StationID} ({fromSlot}) to {toStation.StationID} ({toSlot})."));
            int endEffector = GetSuitableEE(manipulator, fromStation) + 1;
            layout.ManipulatorPick(tID, manipulator.StationID, endEffector, fromStation.StationID, fromSlot);
            layout.ManipulatorPlace(tID, manipulator.StationID, endEffector, toStation.StationID, toSlot);
            if (blockedManipulators.Contains(manipulator))
            {
                lock (lockObject)
                {
                    blockedManipulators.Remove(manipulator);
                }
            }
        }

        private void RunSwap(string tID, Manipulator manipulator, Station pickStation, int pickSlot, Station swapStation, int swapSlot, Station putStation, int putSlot)
        {
            OnLogEvent?.Invoke(this, (tID, $"Swap Initiated for {manipulator.StationID} to swap {swapStation.StationID} ({swapSlot}) with {pickStation.StationID} ({pickSlot}) and put to {putStation.StationID} ({putSlot}).")); 
            layout.ManipulatorPick(tID, manipulator.StationID, 1, pickStation.StationID, pickSlot);
            layout.ManipulatorPick(tID, manipulator.StationID, 2, swapStation.StationID, swapSlot);
            layout.ManipulatorPlace(tID, manipulator.StationID, 1, swapStation.StationID, swapSlot);
            layout.ManipulatorPlace(tID, manipulator.StationID, 2, putStation.StationID, putSlot);
            if (blockedManipulators.Contains(manipulator))
            {
                lock (lockObject)
                {
                    blockedManipulators.Remove(manipulator);
                }
            }
        }

        private (Station station, int slot) GetTargetStationAndSlot(Station targetStation, Payload incomingPayload)
        {
            if (targetStation.PodDockable && !IgnoreLotIDMatching)
            {
                foreach (Station checkStation in layout.StationList.Values)
                {
                    if (checkStation.PodDockable && checkStation.PodID == incomingPayload.LotID)
                        return (checkStation, incomingPayload.StartingSlot);
                }
                throw new Exception();
            }
            else if (targetStation.PodDockable && IgnoreLotIDMatching)
            {
                foreach (Station checkStation in layout.StationList.Values)
                {
                    if(checkStation.PodDockable && (checkStation.AllPayloadsSingularOutputState))
                    {
                        return (checkStation, checkStation.GetNextAvailableSlot());
                    }
                }
                foreach (Station checkStation in layout.StationList.Values)
                {
                    if (checkStation.PodDockable && (checkStation.slots.Count == 0))
                    {
                        return (checkStation, checkStation.GetNextAvailableSlot());
                    }
                }
                throw new Exception();
            }
            else
            {
                return (targetStation, targetStation.GetNextAvailableSlot());
            }
        }

        private void CheckManipulators()
        {
            foreach (string manipulatorID in layout.ManipulatorList.Keys)
            {
                Manipulator manipulator = layout.ManipulatorList[manipulatorID];

                if (manipulator.State != StationState.Idle)
                {
                    manipulator.Tick();
                    continue;
                }
            }
        }
        private Manipulator? CheckAvailableAccessibleManipulators(Station station1, Station station2, Station? station3 = null)
        {
            foreach(Manipulator manipulator in layout.ManipulatorList.Values)
            {
                if (manipulator.State == StationState.Off)
                {
                    manipulator.PowerOn("0");
                }

                try
                {
                    if (blockedManipulators.Contains(manipulator) || manipulator.State != StationState.Idle)
                    {
                        continue;
                    }
                }
                catch (System.InvalidOperationException)
                {
                    continue;
                }

                List<string> manipulatorLocations = [.. manipulator.Locations.Keys];
                List<string> station1Locations = station1.ConcurrentLocationAccess ? [.. station1.Locations.Keys] : [station1.CurrentLocation];
                List<string> station2Locations = station2.ConcurrentLocationAccess ? [.. station2.Locations.Keys] : [station2.CurrentLocation];
                List<string> station3Locations;

                if (!manipulator.EndEffectorTypes.Contains(station1.PayloadType) || !HaveCommonElement(new List<IEnumerable<string>> { station1Locations, station2Locations, manipulatorLocations }))
                {
                    continue;
                }

                if (station3 != null)
                {
                    station3Locations = station3.ConcurrentLocationAccess ? station3.Locations.Keys.ToList() : [station3.CurrentLocation];
                    if (!manipulator.EndEffectorTypes.Contains(station2.PayloadType) || !HaveCommonElement([station2Locations, station3Locations, manipulatorLocations]))
                    {
                        continue;
                    }
                }

                return manipulator;
            }
            return null;
        }
        static bool HaveCommonElement(List<IEnumerable<string>> lists)
        {
            if (lists == null || lists.Count < 2)
                throw new ArgumentException("At least two lists are required.");

            var commonElements = new HashSet<string>(lists[0]);
            foreach (var list in lists.Skip(1))
            {
                commonElements.IntersectWith(list);
                if (commonElements.Count == 0)
                    return false;
            }

            return commonElements.Count > 0;
        }
        static List<string>? GetCommonElements(List<IEnumerable<string>> lists)
        {
            if (lists == null || lists.Count < 2)
                throw new ArgumentException("At least two lists are required.");

            var commonElements = new HashSet<string>(lists[0]);
            foreach (var list in lists.Skip(1))
            {
                commonElements.IntersectWith(list);
                return [.. commonElements];
            }

            return null;
        }
        static bool HasDuplicate(IEnumerable<string> list)
        {
            if (list == null || !list.Any())
                return false;

            var seen = new HashSet<string>();
            foreach (var item in list)
            {
                if (!seen.Add(item))
                    return true; // Duplicate found
            }

            return false; // No duplicates
        }

        public string CreateFilledPod(string tID, int payloadQuantity, int podSize, string payloadType)
        {
            string podID = Layout.GetID(5);
            layout.CreatePod(tID, podID, podSize, payloadType);

            for (int slot = 0; slot < payloadQuantity; slot++)
            {
                layout.CreatePayload(tID, Layout.GetID(6), podID, slot+1);
            }
            return podID;
        }
        public void CreatePodAndDock(string tID, string stationID)
        {
            Station station = layout.StationList[stationID];
            string podID = CreateFilledPod(tID, station.Capacity, station.Capacity, station.PayloadType);
            layout.DockPod(tID, stationID, podID);
            layout.OperateStationDoor(tID, stationID, false);
            while (blockedStations.Contains(station))
            {
                blockedStations.Remove(station);
            }
        }

        private bool IsAnyRobotEnRouteToStation(string stationID)
        {
            foreach (Manipulator manipulator in layout.ManipulatorList.Values)
            {
                if (manipulator.EnRouteStation == stationID || (manipulator.CurrentLocation == stationID && manipulator.ArmState != ManipulatorArmStates.retracted && (manipulator.State != StationState.Idle || manipulator.State != StationState.Off)))
                    { return true; }
            }
            return false;
        }

        private void LogEvent(object? sender, (string? tID, string message) e)
        {
            OnLogEvent?.Invoke(sender, e);
        }

        private void removeThreads()
        {
            foreach(Thread t in Global.RunningThreads.ToList())
            {
                if (!t.IsAlive)
                {
                    Global.RunningThreads.Remove(t);
                }
            }
        }
    }
}
