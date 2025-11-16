using LayoutSimulator.Enums;
using LayoutSimulator.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutSimulator.Models;

public class Reservation
{
    public required EReservationType Type { get; set; }
    public int Id { get; set; } = GenerateId.Instance.GetReservationId();
    public Station? TargetStation { get; set; }
    public required Slot Slot { get; set; }
    public int SlotId { get { return Slot.SlotId; } }
    public required Payload Payload { get; set; }
    public string PayloadType { get { return Payload.PayloadType; } }
}
