using LayoutSimulator.Enums;
using LayoutSimulator.Helpers;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace LayoutSimulator.Models;

public class Manipulator : INotifyPropertyChanged
{
    // REQUIRED PARAMS
    public required string ManipulatorId { get; set; }
    public required Dictionary<int, EndEffector> EndEffectors { get; set; }
    public required List<string> Locations { get; set; }
    public required int MotionTime { get; set; }
    public required int ExtendTime { get; set; }
    public required int RetractTime { get; set; }


    // OTHER PARAMS
    private EManipulatorState state = EManipulatorState.Off;
    public EManipulatorState State
    {
        get { return state; }
        set { state = value;  OnPropertyChanged(); }
    }

    private string currentLocation;
    public string CurrentLocationStationId
    {
        get { return currentLocation; }
        set { currentLocation = value; OnPropertyChanged(); }
    }


    public Manipulator()
    {
        if (Locations is null || Locations.Count == 0)
            throw new ErrorResponse(EErrorCode.MissingArguments, $"No Locations for Manipulator {ManipulatorId}");
        currentLocation = Locations[0];
    }

    private void CheckBusy()
    {
        if (State != EManipulatorState.Idle)
            throw new ErrorResponse(EErrorCode.ActionWhileBusy, $"Manipulator {ManipulatorId} is busy.");

        foreach(KeyValuePair<int, EndEffector> EE  in EndEffectors)
        {
            if (EE.Value.ArmState != EManipulatorArmState.retracted)
                throw new ErrorResponse(EErrorCode.UnknownArmState, $"Manipulator {ManipulatorId} has Arm {EE.Key} Extended while idle.");
        }
    }
    private void CheckTransferCompatibility(int endEffectorId, List<Reservation> reservations)
    {
        if (!EndEffectors.ContainsKey(endEffectorId))
            throw new ErrorResponse(EErrorCode.EndEffectorOutOfBounds, $"Manipulator {ManipulatorId} does not contain end effector {endEffectorId}");

        if (reservations.Count == 0)
            throw new ErrorResponse(EErrorCode.InvalidReservation, "No Reservations provided");

        if (reservations.Count > EndEffectors[endEffectorId].PayloadSlots)
            throw new ErrorResponse(EErrorCode.InvalidReservation, $"Too many Reservations provided for Manipulator {ManipulatorId} End Effector {endEffectorId}");

        if (reservations.Count != EndEffectors[endEffectorId].PayloadSlots)
            Log.Instance.Info(new LogMessage("", $"Manipulator {ManipulatorId} End Effector {endEffectorId} received {reservations.Count} Reservations but has {EndEffectors[endEffectorId].PayloadSlots} Payload Slots"));

        if (reservations.Any(r => r.TargetStation == null))
            throw new ErrorResponse(EErrorCode.ProgramError, "One or more Reservations have no Target Station set");

        foreach (Reservation reservation in reservations)
        {
            if (reservation.TargetStation == null)
                throw new ErrorResponse(EErrorCode.ProgramError, "Target Station is not set");
        }

        string expectedPayloadId = reservations.FirstOrDefault()!.Payload.PayloadID;
        string expectedTargetStationId = reservations.FirstOrDefault()!.TargetStation!.StationId;
        EReservationType expectedReservationType = reservations.FirstOrDefault()!.Type;

        foreach (Reservation reservation in reservations)
        {
            if ((expectedPayloadId != reservation.Payload.PayloadID) || (expectedTargetStationId != reservation.TargetStation!.StationId) || (expectedReservationType != reservation.Type))
                throw new ErrorResponse(EErrorCode.InvalidReservation, $"Reservations did not match");
        }

        if (!Locations.Intersect(reservations.FirstOrDefault()!.TargetStation!.Locations.Keys).Any())
            throw new ErrorResponse(EErrorCode.StationNotReachable, $"Manipulator {ManipulatorId} could not access any locations.");

        if (EndEffectors[endEffectorId].PayloadType != reservations.FirstOrDefault()!.PayloadType)
            throw new ErrorResponse(EErrorCode.PayloadTypeMismatch, $"Manipulator {ManipulatorId} End Effector did not match the payload type for this station.");
    }


    private void GoToStation(string tID, string stationId)
    {
        if (CurrentLocationStationId != stationId)
        {
            State = EManipulatorState.Moving;
            InternalClock.Instance.ProcessWait(MotionTime);
            CurrentLocationStationId = stationId;
            Log.Instance.Info(new LogMessage(tID, $"Manipulator {ManipulatorId} Moved to Station {stationId}"));
        }
    }
    private void ExtendArm(string tID, int endEffectorId)
    {
        if (!EndEffectors.ContainsKey(endEffectorId))
            throw new ErrorResponse(EErrorCode.EndEffectorOutOfBounds, $"End Effector {endEffectorId} is not valid");

        if (EndEffectors[endEffectorId].ArmState == EManipulatorArmState.extended)
            throw new ErrorResponse(EErrorCode.IncorrectState, $"End Effector {endEffectorId} was already extended");

        State = EManipulatorState.Extending;
        InternalClock.Instance.ProcessWait(ExtendTime);
        EndEffectors[endEffectorId].ArmState = EManipulatorArmState.extended;
    }
    private void RetractArm(string tID, int endEffectorId)
    {
        if (!EndEffectors.ContainsKey(endEffectorId))
            throw new ErrorResponse(EErrorCode.EndEffectorOutOfBounds, $"End Effector {endEffectorId} is not valid");

        if (EndEffectors[endEffectorId].ArmState == EManipulatorArmState.retracted)
            throw new ErrorResponse(EErrorCode.IncorrectState, $"End Effector {endEffectorId} was already retracted");

        State = EManipulatorState.Retracting;
        InternalClock.Instance.ProcessWait(RetractTime);
        EndEffectors[endEffectorId].ArmState = EManipulatorArmState.retracted;
    }

    public void Home(string tID)
    {
        CheckBusy();
        Log.Instance.Info(new LogMessage(tID, $"Manipulator {ManipulatorId} Homing"));
        GoToStation(tID, "home");
        State = EManipulatorState.Idle;
        Log.Instance.Info(new LogMessage(tID, $"Manipulator {ManipulatorId} at Home"));
    }
    public void PowerOff(string tID)
    {
        CheckBusy();
        State = EManipulatorState.Off;
        Log.Instance.Info(new LogMessage(tID, $"Manipulator {ManipulatorId} Off."));
    }
    public void PowerOn(string tID)
    {
        if (State != EManipulatorState.Off && State != EManipulatorState.Idle)
            throw new ErrorResponse(EErrorCode.IncorrectState, "Invalid State");
        State = EManipulatorState.Idle;
        Log.Instance.Info(new LogMessage(tID, $"Manipulator {ManipulatorId} On"));
    }
    public void Pick(string tID, int endEffectorId, List<Reservation> reservations)
    {
        CheckBusy();
        CheckTransferCompatibility(endEffectorId, reservations);
        if (reservations.FirstOrDefault()!.Type != EReservationType.pickFromStation)
            throw new ErrorResponse(EErrorCode.InvalidReservation, $"Manipulator {ManipulatorId} Pick did not receieve a pick reservation");
        

        PickOrPlace(tID, endEffectorId, reservations);
    }
    public void Place(string tID, int endEffectorId, List<Reservation> reservations)
    {
        CheckBusy();
        CheckTransferCompatibility(endEffectorId, reservations);
        if (reservations.FirstOrDefault()!.Type != EReservationType.placeInStation)
            throw new ErrorResponse(EErrorCode.InvalidReservation, $"Manipulator {ManipulatorId} Place did not receieve a place reservation");
        

        PickOrPlace(tID, endEffectorId, reservations);
    }

    private void PickOrPlace(string tID, int endEffectorId, List<Reservation> reservations)
    {
        State = EManipulatorState.Moving;
        GoToStation(tID, reservations.FirstOrDefault()!.TargetStation!.StationId);

        State = EManipulatorState.Extending;

        HashSet<string> commonElements = new(Locations);
        commonElements.IntersectWith(reservations.FirstOrDefault()!.TargetStation!.Locations.Keys);
        reservations.FirstOrDefault()!.TargetStation!.CheckLocationAccess(commonElements.First());

        reservations.FirstOrDefault()!.TargetStation!.StartAccess(tID, reservations);
        ExtendArm(tID, endEffectorId);

        if (reservations.First()!.Type == EReservationType.pickFromStation)
        {
            EndEffectors[endEffectorId].Payloads = reservations.First()!.TargetStation!.PickFromStation(tID, reservations);
        }
        else
        {
            reservations.First()!.TargetStation!.PlaceInStation(tID, reservations);
            EndEffectors[endEffectorId].Payloads = [];
        }

        State = EManipulatorState.Retracting;
        RetractArm(tID, endEffectorId);

        reservations.First()!.TargetStation!.StopAccess(tID, reservations);
        State = EManipulatorState.Idle;
    }




    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
