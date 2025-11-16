using LayoutSimulator.Enums;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LayoutSimulator.Models;

public class Station : INotifyPropertyChanged
{
    // REQUIRED PARAMS
    public required string StationId { get; set; }
    public required bool AutoMode { get; set; }
    public required Cassette? Cassette { get; set; }
    public required Dictionary<string, Access> Locations { get; set; }
    public required Dictionary<string, Process> Processs { get; set; }
    public required bool IsInputAndPodDockable { get; set; }
    public required bool IsOutputAndPodDockable { get; set; }


    // OTHER PARAMS
    private EStationState state;
    public EStationState State
    {
        get { return state; }
        set 
        { 
            state = value; 
            OnPropertyChanged(); 
        }
    }
    private string? podId = null;
    public string PodId
    {
        get
        {
            if (!PodDockable)
                throw new ErrorResponse(EErrorCode.NotPodDockable, $"Station {StationId} is not a Pod Dockable station.");
            if (podId != null)
                return podId;
            else
                throw new ErrorResponse(EErrorCode.PodNotAvailable, $"Station {StationId} does not have a Pod.");
        }
        set
        {
            if (value == string.Empty)
                podId = null;
            podId = value; OnPropertyChanged();
        }
    }
    protected internal bool PodDockable { get; set; }
    public List<int> PendingReservationIds { get; set; } = [];

    public Station()
    {
        PodDockable = IsInputAndPodDockable || IsOutputAndPodDockable;

        if (!PodDockable && Cassette == null)
            throw new ErrorResponse(EErrorCode.PodNotAvailable, $"Station {StationId} does not contain expected cassette");
    }

    private void CheckPod()
    {
        if (Cassette == null || PodId == null)
            throw new ErrorResponse(EErrorCode.PodNotAvailable, $"Station {StationId} did not contain Pod");
    }
    protected internal void CheckLocationAccess(string location)
    {
        if (!Locations.ContainsKey(location))
            throw new ErrorResponse(EErrorCode.NotAccessible, $"Location {location} is Out of Bounds");

        if (!AutoMode && Locations[location].DoorStatus != EDoorStatus.Open)
            throw new ErrorResponse(EErrorCode.NotAccessible, $"Door to Location {location} is not open.");
    }
    private void CheckReservations(List<Reservation> reservations)
    {
        foreach (Reservation reservation in reservations)
        {
            if (!PendingReservationIds.Contains(reservation.Id))
                throw new ErrorResponse(EErrorCode.InvalidReservation, $"Reservation {reservation.Id} not found in Waiting List");
        }
    }



    public void DockPod(Pod pod)
    {
        if (State != EStationState.Idle)
            throw new ErrorResponse(EErrorCode.ActionWhileBusy, $"Station {StationId} is Busy");

        if (Cassette != null)
            throw new ErrorResponse(EErrorCode.PodAlreadyAvailable, $"Station {StationId} already contains Pod {PodId}");

        PodId = pod.PodID;
        Cassette = pod.Cassette;
    }
    public Pod UndockPod()
    {
        if (State != EStationState.Idle)
            throw new ErrorResponse(EErrorCode.ActionWhileBusy, $"Station {StationId} is Busy");
        CheckPod();
        

        Pod returnPod = new()
        {
            PodID = PodId,
            Cassette = Cassette!
        };

        PodId = string.Empty;
        Cassette = null;

        return returnPod;
    }

    private void AdjustAndAddReservationsToWaitingList(string tID, IEnumerable<Reservation> reservations)
    {
        foreach (Reservation reservation in reservations)
        {
            reservation.TargetStation = this;
            PendingReservationIds.Add(reservation.Id);
        }
            
    }
    private void RemoveReservationFromWaitingList(string tID, List<Reservation> reservations)
    {
        foreach (Reservation reservation in reservations)
        {
            if (PendingReservationIds.Contains(reservation.Id))
                PendingReservationIds.Remove(reservation.Id);
            else
                throw new ErrorResponse(EErrorCode.IncorrectState, $"Reservation {reservation.Id} was not found in the pending list");
        }
    }

    protected internal List<Reservation> ReservePickFromStation(string tID, int startingSlot = 0, int slotCount = 1)
    {
        if (State != EStationState.Idle)
            throw new ErrorResponse(EErrorCode.ActionWhileBusy, $"Station {StationId} is Busy");

        if (Cassette == null)
            throw new ErrorResponse(EErrorCode.PodNotAvailable, $"Station {StationId} does not contain expected cassette");

        if (startingSlot == 0)
            startingSlot = Cassette.GetNextOccupiedSlot();

        if (startingSlot < 0)
            throw new ErrorResponse(EErrorCode.SlotOutOfBounds, $"Starting slot cannot be less than 1. Or you did not pass in a starting slot and all slots are unavailable.");

        List<Reservation> reservations = Cassette.SetReservation(EReservationType.pickFromStation, startingSlot, slotCount);
        AdjustAndAddReservationsToWaitingList(tID, reservations);
        State = EStationState.WaitingToBeAccessed;
        return reservations;
    }
    protected internal List<Reservation> ReservePlaceToStation(string tID, List<Payload> payloads, int startingSlot = 0, int slotCount = 1)
    {
        if (State != EStationState.Idle)
            throw new ErrorResponse(EErrorCode.ActionWhileBusy, $"Station {StationId} is Busy");

        if (Cassette == null)
            throw new ErrorResponse(EErrorCode.PodNotAvailable, $"Station {StationId} does not contain expected cassette");

        if (startingSlot == 0)
            startingSlot = Cassette.GetNextEmptySlot();

        if (startingSlot < 0)
            throw new ErrorResponse(EErrorCode.SlotOutOfBounds, $"Starting slot cannot be less than 1. Or you did not pass in a starting slot and all slots are unavailable.");

        List<Reservation> reservations = Cassette.SetReservation(EReservationType.placeInStation, startingSlot, slotCount, payloads);
        AdjustAndAddReservationsToWaitingList(tID, reservations);
        State = EStationState.WaitingToBeAccessed;
        return reservations;
    }

    protected internal List<Payload> PickFromStation(string tID, List<Reservation> reservations)
    {
        if (State != EStationState.BeingAccessed)
            throw new ErrorResponse(EErrorCode.IncorrectState, "Station Expected to be switched to Being Accessed.");
        CheckPod();
        CheckReservations(reservations);

        List<Payload> outputPayloads = new List<Payload>();
        foreach (Reservation reservation in reservations)
        {
            outputPayloads.Add(Cassette!.RemovePayload(reservation));
        }
        return outputPayloads;
    }
    protected internal void PlaceInStation(string tID, List<Reservation> reservations)
    {
        if (State != EStationState.BeingAccessed)
            throw new ErrorResponse(EErrorCode.IncorrectState, "Station Expected to be switched to Being Accessed.");
        CheckPod();
        CheckReservations(reservations);

        foreach (Reservation reservation in reservations)
        {
            Cassette!.AddPayload(reservation);
        }
    }

    protected internal void StartAccess(string tID, List<Reservation> reservations)
    {
        if (State != EStationState.WaitingToBeAccessed)
            throw new ErrorResponse(EErrorCode.IncorrectState, "Station Expected to be waiting for Payload.");
        CheckReservations(reservations);

        State = EStationState.BeingAccessed;
    }
    protected internal void StopAccess(string tID, List<Reservation> reservations)
    {
        RemoveReservationFromWaitingList(tID, reservations);
        if (PendingReservationIds.Count == 0)
            State = EStationState.Idle;
    }


    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
