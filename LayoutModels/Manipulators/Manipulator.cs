using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using LayoutModels.Stations;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace LayoutModels.Manipulators
{
    public enum ManipulatorArmStates
    {
        extended,
        retracted
    }

    public class Manipulator : BaseStation, ITarget
    {
        // EVENTS
        public event EventHandler<bool>? OnPowerEvent;
        public event EventHandler<bool>? ArmExtendEvent;
        public event EventHandler<bool>? ArmRetractEvent;
        public event EventHandler<string>? OnBegingLocationChangeEvent;
        public event EventHandler<string>? OnLocationChangeEvent;
        public event EventHandler<(int, string, Payload)>? OnPickUp;
        public event EventHandler<(int, string)>? OnDropOff;

        // PROPERTIES
        public Dictionary<int, Dictionary<string, Payload>> EndEffectors { get; private set; }
        public List<string> EndEffectorTypes { get; set; }
        public float MotionTime { get; private set; }
        public float ExtendTime { get; private set; }
        public float RetractTime { get; private set; }
        public string? EnRouteStation { get; private set; } = null;
        public ManipulatorArmStates ArmState { get; private set; } = ManipulatorArmStates.retracted;

        // CONSTRUCTORS
        public Manipulator(string manipulatorID, string ManipulatorType, Dictionary<int, Dictionary<string, Payload>> endEffectors, List<string> endEffectorsTypes, List<string> locations, float motionTime, float extendTime, float retractTime) : base(manipulatorID, ManipulatorType, locations)
        {
            EndEffectors = endEffectors;
            EndEffectorTypes = endEffectorsTypes;
            MotionTime = motionTime;
            ExtendTime = extendTime;
            RetractTime = retractTime;
            State = StationState.Off;
        }

        // INTERNAL PROCESSES
        private void GoToStation(string tID, string stationID)
        {
            if (CurrentLocation != stationID)
            {
                EnRouteStation = stationID;
                OnBegingLocationChangeEvent?.Invoke(this, stationID);
                ProcessWait(tID, MotionTime);
                CurrentLocation = stationID;
                EnRouteStation = null;
                OnLocationChangeEvent?.Invoke(this, stationID);
            }
        }
        private void Extend(string tID)
        {
            ArmRetractEvent?.Invoke(this, false);
            ProcessWait(tID, ExtendTime);
            ArmState = ManipulatorArmStates.extended;
            ArmExtendEvent?.Invoke(this, true);
        }
        private void Retract(string tID)
        {
            ArmExtendEvent?.Invoke(this, false);
            ProcessWait(tID, RetractTime);
            ArmState = ManipulatorArmStates.retracted;
            ArmRetractEvent?.Invoke(this, true);
        }

        // COMMANDS
        public void Pick(string tID, int endEffector, Station station, int slot)
        {
            if (EndEffectors[endEffector].ContainsKey("payload"))
                throw new ErrorResponse(ErrorCodes.PayloadAlreadyAvailable, $"Manipulator {StationID} End Effector {endEffector} did not contain payload.");

            if (!Locations.Keys.Intersect(station.Locations.Keys).Any())
                throw new ErrorResponse(ErrorCodes.StationNotReachable, $"Manipulator {StationID} could not access any locations.");

            if (EndEffectorTypes[endEffector - 1] != station.PayloadType)
                throw new ErrorResponse(ErrorCodes.PayloadTypeMismatch, $"Manipulator {StationID} End Effector did not match the payload type for this station.");

            if (ArmState != ManipulatorArmStates.retracted)
                throw new ErrorResponse(ErrorCodes.UnknownArmState, $"Manipulator {StationID} arm was not retracted. Home the Manipulator.");

            if (slot == 0)
                slot = station.GetNextAvailableSlot();

            if (slot > station.Capacity)
                throw new ErrorResponse(ErrorCodes.SlotIndexMissing, $"Manipulator {StationID} did not recognize slot {slot}.");

            if (station.CheckSlotEmpty(slot))
                throw new ErrorResponse(ErrorCodes.PayloadNotAvailable, $"Manipulator {StationID} slot {slot} access on was empty.");

            PickPlace(true, tID, endEffector, station, slot);

            OnPickUp?.Invoke(this, (endEffector, station.StationID, EndEffectors[endEffector]["payload"]));
        }
        public void Place(string tID, int endEffector, Station station, int slot)
        {
            if (!EndEffectors[endEffector].TryGetValue("payload", out Payload? value))
                throw new ErrorResponse(ErrorCodes.PayloadNotAvailable, $"Manipulator {StationID} End Effector {endEffector} did not contain payload.");

            if (!Locations.Keys.Intersect(station.Locations.Keys).Any())
                throw new ErrorResponse(ErrorCodes.StationNotReachable, $"Manipulator {StationID} could not access any locations.");

            if (ArmState != ManipulatorArmStates.retracted)
                throw new ErrorResponse(ErrorCodes.UnknownArmState, $"Manipulator {StationID} arm was not retracted. Home the Manipulator.");

            if (!station.CheckPayloadCompatible(value))
                throw new ErrorResponse(ErrorCodes.PayloadTypeMismatch, $"Manipulator {StationID} End Effector did not match the payload type for this station.");

            if (slot == 0)
                slot = station.GetNextEmptySlot();

            if (slot > station.Capacity)
                throw new ErrorResponse(ErrorCodes.SlotIndexMissing, $"Manipulator {StationID} did not recognize slot {slot}.");

            if (!station.CheckSlotEmpty(slot))
                throw new ErrorResponse(ErrorCodes.PayloadAlreadyAvailable, $"Manipulator {StationID} slot {slot} access on was empty.");

            PickPlace(false, tID, endEffector, station, slot);

            OnDropOff?.Invoke(this, (endEffector, station.StationID));
        }
        private void PickPlace(bool pickNotPlace, string tID, int endEffector, Station station, int slot)
        {
            State = StationState.Moving;
            GoToStation(tID, station.StationID);
            State = StationState.Extending;

            HashSet<string> commonElements = new(Locations.Keys);
            commonElements.IntersectWith(station.Locations.Keys);

            if (station.ConcurrentLocationAccess && station.State != StationState.Idle)
            {
                while (station.State != StationState.Idle)
                {
                    Thread.Sleep(1);
                }
            }

            station.StartStationAccess(commonElements.First());
            Extend(tID);

            if (pickNotPlace)
            {
                EndEffectors[endEffector]["payload"] = station.ReleasePayload(tID, slot);
            }
            else
            {
                station.AcceptPayload(tID, EndEffectors[endEffector]["payload"], slot);
                EndEffectors[endEffector].Remove("payload");
            }

            State = StationState.Retracting;
            Retract(tID);

            station.StopStationAccess();
            State = StationState.Idle;
        }
        public void Home(string tID)
        {
            State = StationState.Moving;

            Log(tID, $"Manipulator {StationID} Homing");

            if (ArmState != ManipulatorArmStates.retracted)
                Retract(tID);

            GoToStation(tID, "home");
            State = StationState.Idle;
            Log(tID, $"Manipulator {StationID} at Home");
        }
        public void PowerOff(string tID)
        {
            if (State != StationState.Idle)
                throw new ErrorResponse(ErrorCodes.PowerOffWhileBusy, $"Manipulator {StationID} was busy.");
            State = StationState.Off;
            Log(tID, $"Manipulator {StationID} Off.");
            OnPowerEvent?.Invoke(this, false);
        }
        public void PowerOn(string tID)
        {
            if (State != StationState.Idle && State != StationState.Off)
                throw new ErrorResponse(ErrorCodes.ProgramError, $"Manipulator {StationID} was busy.");

            State = StationState.Idle;
            Log(tID, $"Manipulator {StationID} On");
            OnPowerEvent?.Invoke(this, true);
        }
    }
}
