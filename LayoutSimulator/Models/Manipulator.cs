using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutSimulator.Models;

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
    public EManipulatorArmState ArmState { get; private set; } = EManipulatorArmState.retracted;

    // CONSTRUCTORS
    public Manipulator(string manipulatorID, string ManipulatorType, Dictionary<int, Dictionary<string, Payload>> endEffectors, List<string> endEffectorsTypes, List<string> locations, float motionTime, float extendTime, float retractTime) : base(manipulatorID, ManipulatorType, locations)
    {
        EndEffectors = endEffectors;
        EndEffectorTypes = endEffectorsTypes;
        MotionTime = motionTime;
        ExtendTime = extendTime;
        RetractTime = retractTime;
        State = EStationState.Off;
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
        ArmState = EManipulatorArmState.extended;
        ArmExtendEvent?.Invoke(this, true);
    }
    private void Retract(string tID)
    {
        ArmExtendEvent?.Invoke(this, false);
        ProcessWait(tID, RetractTime);
        ArmState = EManipulatorArmState.retracted;
        ArmRetractEvent?.Invoke(this, true);
    }

    // COMMANDS
    public void Pick(string tID, int endEffector, Station station, int slot)
    {
        if (EndEffectors[endEffector].ContainsKey("payload"))
            throw new ErrorResponse(EErrorCode.PayloadAlreadyAvailable, $"Manipulator {StationID} End Effector {endEffector} did not contain payload.");

        if (!Locations.Keys.Intersect(station.Locations.Keys).Any())
            throw new ErrorResponse(EErrorCode.StationNotReachable, $"Manipulator {StationID} could not access any locations.");

        if (EndEffectorTypes[endEffector - 1] != station.PayloadType)
            throw new ErrorResponse(EErrorCode.PayloadTypeMismatch, $"Manipulator {StationID} End Effector did not match the payload type for this station.");

        if (ArmState != EManipulatorArmState.retracted)
            throw new ErrorResponse(EErrorCode.UnknownArmState, $"Manipulator {StationID} arm was not retracted. Home the Manipulator.");

        if (slot == 0)
            slot = station.GetNextAvailableSlot();

        if (slot > station.Capacity)
            throw new ErrorResponse(EErrorCode.SlotIndexMissing, $"Manipulator {StationID} did not recognize slot {slot}.");

        if (station.CheckSlotEmpty(slot))
            throw new ErrorResponse(EErrorCode.PayloadNotAvailable, $"Manipulator {StationID} slot {slot} access on was empty.");

        PickPlace(true, tID, endEffector, station, slot);

        OnPickUp?.Invoke(this, (endEffector, station.StationID, EndEffectors[endEffector]["payload"]));
    }
    public void Place(string tID, int endEffector, Station station, int slot)
    {
        if (!EndEffectors[endEffector].TryGetValue("payload", out Payload? value))
            throw new ErrorResponse(EErrorCode.PayloadNotAvailable, $"Manipulator {StationID} End Effector {endEffector} did not contain payload.");

        if (!Locations.Keys.Intersect(station.Locations.Keys).Any())
            throw new ErrorResponse(EErrorCode.StationNotReachable, $"Manipulator {StationID} could not access any locations.");

        if (ArmState != EManipulatorArmState.retracted)
            throw new ErrorResponse(EErrorCode.UnknownArmState, $"Manipulator {StationID} arm was not retracted. Home the Manipulator.");

        if (!station.CheckPayloadCompatible(value))
            throw new ErrorResponse(EErrorCode.PayloadTypeMismatch, $"Manipulator {StationID} End Effector did not match the payload type for this station.");

        if (slot == 0)
            slot = station.GetNextEmptySlot();

        if (slot > station.Capacity)
            throw new ErrorResponse(EErrorCode.SlotIndexMissing, $"Manipulator {StationID} did not recognize slot {slot}.");

        if (!station.CheckSlotEmpty(slot))
            throw new ErrorResponse(EErrorCode.PayloadAlreadyAvailable, $"Manipulator {StationID} slot {slot} access on was empty.");

        PickPlace(false, tID, endEffector, station, slot);

        OnDropOff?.Invoke(this, (endEffector, station.StationID));
    }
    private void PickPlace(bool pickNotPlace, string tID, int endEffector, Station station, int slot)
    {
        State = EStationState.Moving;
        GoToStation(tID, station.StationID);
        State = EStationState.Extending;

        HashSet<string> commonElements = new(Locations.Keys);
        commonElements.IntersectWith(station.Locations.Keys);

        if (station.ConcurrentLocationAccess && station.State != EStationState.Idle)
        {
            while (station.State != EStationState.Idle)
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

        State = EStationState.Retracting;
        Retract(tID);

        station.StopStationAccess();
        State = EStationState.Idle;
    }
    public void Home(string tID)
    {
        State = EStationState.Moving;

        Log(new LogMessage(tID, $"Manipulator {StationID} Homing"));

        if (ArmState != EManipulatorArmState.retracted)
            Retract(tID);

        GoToStation(tID, "home");
        State = EStationState.Idle;
        Log(new LogMessage(tID, $"Manipulator {StationID} at Home"));
    }
    public void PowerOff(string tID)
    {
        if (State != EStationState.Idle)
            throw new ErrorResponse(EErrorCode.PowerOffWhileBusy, $"Manipulator {StationID} was busy.");
        State = EStationState.Off;
        Log(new LogMessage(tID, $"Manipulator {StationID} Off."));
        OnPowerEvent?.Invoke(this, false);
    }
    public void PowerOn(string tID)
    {
        if (State != EStationState.Idle && State != EStationState.Off)
            throw new ErrorResponse(EErrorCode.ProgramError, $"Manipulator {StationID} was busy.");

        State = EStationState.Idle;
        Log(new LogMessage(tID, $"Manipulator {StationID} On"));
        OnPowerEvent?.Invoke(this, true);
    }
}
