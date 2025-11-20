using LayoutSimulator.Enums;
using LayoutSimulator.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LayoutSimulator.Models;

public class Cassette : INotifyPropertyChanged
{
    // REQUIRED PARAMETERS
    public required int Capacity { get; set; }
    public required string PayloadType { get; set; }
    public required bool IsMovableCassette { get; set; }
    public uint SlotMoveTime { get; set; }

    // OTHER PARAMETERS
    public Dictionary<int, Slot> Slots { get; private set; } = [];
    private int currentSlot;
    public int CurrentSlot
    {
        get { return currentSlot; }
        set { currentSlot = value;
            OnPropertyChanged();
        }
    }

    private bool readyToAccess = true;
    public bool ReadyToAccess
    {
        get { return readyToAccess; }
        set { readyToAccess = value;
            OnPropertyChanged();
        }
    }
    protected internal int CurrentCapacity
    {
        get
        {
            int count = 0;
            foreach (KeyValuePair<int, Slot> kvp in Slots)
            {
                if (kvp.Value.IsOccupied)
                    count++;
            }
            return count;
        }
    }
    protected internal bool AllSlotsHavePayloadsWithSamePayloadState
    {
        get
        {
            string payloadState = string.Empty;
            foreach (KeyValuePair<int, Slot> kvp in Slots)
            {
                if (!kvp.Value.IsOccupied)
                {
                    if (payloadState == string.Empty)
                        payloadState = kvp.Value.Payload!.PayloadState;

                    if (kvp.Value.Payload!.PayloadState != payloadState)
                        return false;
                }
            }
            if (payloadState == string.Empty)
                return false;
            return true;
        }
    }
    protected internal string? PayloadStateOfWafersInSlots
    {
        get
        {
            if (!AllSlotsHavePayloadsWithSamePayloadState)
                return null;

            string payloadState = string.Empty;
            foreach (KeyValuePair<int, Slot> kvp in Slots)
            {
                if (kvp.Value.IsOccupied)
                {
                    return kvp.Value.Payload!.PayloadState;
                }
            }
            if (payloadState == string.Empty)
                return null;
            return payloadState;
        }
    }


    public Cassette()
    {
        if (Capacity < 1)
            throw new ErrorResponse(EErrorCode.ProgramError, $"Cassette cannot have capacity less than 1.");

        if (Slots.Count == 0)
        {
            for (int i = 1; i <= Capacity; i++)
            {
                Slots.Add(i, new Slot(i));
            }
        }

        if (Slots.Count != Capacity)
            throw new ErrorResponse(EErrorCode.ProgramError, $"Cassette capacity {Capacity} does not match number of slots {Slots.Count}.");

        if (IsMovableCassette && SlotMoveTime == 0)
            throw new ErrorResponse(EErrorCode.ProgramError, $"Time not set for Movable Cassette.");
    }

    public int GetNextEmptySlot()
    {
        foreach (KeyValuePair<int, Slot> kvp in Slots)
        {
            if (!kvp.Value.IsOccupied)
            {
                return kvp.Key;
            }
        }
        return -1;
    }
    public int GetNextOccupiedSlot()
    {
        foreach (KeyValuePair<int, Slot> kvp in Slots)
        {
            if (kvp.Value.IsOccupied)
            {
                return kvp.Key;
            }
        }
        return -1;
    }


    // INTERLOCK LOGIC
    private void SlotInterlock(int slot)
    {
        if (slot < 1)
            throw new ErrorResponse(EErrorCode.SlotsNotEmpty, $"Cassette has No Available Slots");

        else if (slot > Capacity)
            throw new ErrorResponse(EErrorCode.SlotOutOfBounds, $"Cassette has only {Capacity} slots. Slot {slot} is out of bounds.");
    }
    private void PayloadInterlock(Payload payload)
    {
        if (PayloadType != payload.PayloadType)
            throw new ErrorResponse(EErrorCode.PayloadTypeMismatch, $"Cassette accepts {PayloadType} and not {payload.PayloadType}.");
    }



    protected internal List<Reservation> SetReservation (EReservationType reservationType, int startingSlot, int slotCount, List<Payload>? payloads = null)
    {
        if (slotCount <= 0)
            throw new ErrorResponse(EErrorCode.SlotOutOfBounds, "Slot Count cannot be 0 or less.");

        if (payloads != null && payloads.Count != 0 && payloads.Count != slotCount)
            throw new ErrorResponse(EErrorCode.MissingArguments, "Slot Count does not match the payloads passed in");

        if (payloads != null)
        {
            foreach (Payload payload in payloads)
            {
                PayloadInterlock(payload);
            }
        }
        List<Reservation> reservations = new();

        for (int i = startingSlot; i < startingSlot + slotCount; i++)
        {
            SlotInterlock(i);
            Payload? payload;
            if (payloads == null)
                payload = null;
            else if(payloads.Count == 0)
                payload = null;
            else
                payload = payloads[i - startingSlot];
            //reservations[i] = Slots[i].SetReservation(reservationType, payload);
            reservations.Add(Slots[i].SetReservation(reservationType, payload));
        }
        return reservations;
    }

    protected internal void AddPayloadWithoutReservation(int slot, Payload payload)
    {
        SlotInterlock(slot);
        PayloadInterlock(payload);
        Slots[slot].AddPayload(payload);
    }

    protected internal void AddPayload(Reservation reservation)
    {
        SlotInterlock(reservation.SlotId);
        PayloadInterlock(reservation.Payload);
        Slots[reservation.SlotId].AddPayload(reservation.Payload, reservation);
    }
    protected internal Payload RemovePayload (Reservation reservation)
    {
        SlotInterlock(reservation.SlotId);
        return Slots[reservation.SlotId].RemovePayload(reservation);
    }

    protected internal void MoveToSlot(int slot)
    {
        if (!IsMovableCassette)
            throw new ErrorResponse(EErrorCode.CassetteNotMovable, "Cassette is not movable");

        uint slotsToMove = (uint)Math.Abs(CurrentSlot - slot);
        ReadyToAccess = false;
        InternalClock.Instance.ProcessWait(slotsToMove * SlotMoveTime);
        ReadyToAccess= true;
    }

    protected internal void UpdatePayloadState(string newPayloadState)
    {
        foreach (KeyValuePair<int, Slot> kvp in Slots)
        {
            if (kvp.Value.IsOccupied)
            {
                kvp.Value.Payload!.PayloadState = newPayloadState;
            }
        }
    }



    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
