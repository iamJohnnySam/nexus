using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LayoutSimulator.Models;

public class Payload: INotifyPropertyChanged
{
    public event EventHandler<LogMessage>? OnLogEvent;
    public event EventHandler<string>? OnStateChanged;

    public required string PayloadID { get; set; }
    public required string PayloadType { get; set; }
    public required string LotID { get; set; }
    public required int SlotInLot { get; set; }


    private string currentStationId = string.Empty;
    public string CurrentStationId
    {
        get { return currentStationId; }
        set
        {
            currentStationId = value;
            OnLogEvent?.Invoke(this, new LogMessage($"Payload {PayloadID} current station has been updated to {value}"));
            OnPropertyChanged();
        }
    }

    private int currentSlotId;
    public int CurrentSlotId
    {
        get { return currentSlotId; }
        set
        {
            currentSlotId = value;
            OnLogEvent?.Invoke(this, new LogMessage($"Payload {PayloadID} current slot has been updated to {value}"));
            OnPropertyChanged();
        }
    }

    private bool payloadErrorStaus = false;
    public bool PayloadErrorStaus
    {
        get { return payloadErrorStaus; }
        set
        {
            payloadErrorStaus = value;
            OnLogEvent?.Invoke(this, new LogMessage($"Payload {PayloadID} error state updated to {value}"));
            OnPropertyChanged();
        }
    }

    private string payloadState = "unprocessed";
    public string PayloadState
    {
        get { return payloadState; }
        set
        {
            payloadState = value;
            OnLogEvent?.Invoke(this, new LogMessage($"Payload {PayloadID} state updated to {value}"));
            OnStateChanged?.Invoke(this, payloadState);
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

}
