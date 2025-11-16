using LayoutSimulator.Enums;
using LayoutSimulator.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LayoutSimulator.Models;

public class Reader : INotifyPropertyChanged
{
    // PROPERTIES
    public required string ReaderId { get; set; }
    public required Station TargetStation { get; set; }
    public int SlotId { get; set; } = 0;
    public required EReaderType Type { get; set; }


    public Reader()
    {
        if (TargetStation == null)
            throw new ErrorResponse(EErrorCode.ProgramError, "Target Station not assigned for Reader");

        if(Type == EReaderType.Payload)
        {
            if (SlotId == 0)
                throw new ErrorResponse(EErrorCode.MissingArguments, $"No Slot assigned for reader for Target Station {TargetStation.StationId}");
            // TargetStation.PairReader(readerID, slot);
        }
        else
        {
            if (TargetStation.PodDockable)
            {
                // targetStation.PairReader(readerID);
            }
            else
            {

            }
            
        }
    }

    public string ReadID(string transactionID)
    {
        string value;
        if (Type == EReaderType.Payload)
        {
            if (TargetStation.Cassette.Slots[SlotId].IsOccupied)
            {
                value = TargetStation.Cassette.Slots[SlotId].Payload!.PayloadID;
                Log.Instance.Info(new LogMessage(transactionID, $"Reader {ReadID} returned slot Id {value} at {TargetStation.StationId}"));
            }
            else
                throw new ErrorResponse(EErrorCode.PayloadNotAvailable, $"Reader {ReaderId} did not have any payload on {TargetStation.StationId} slot {SlotId} to read.");
        }
        else
        {
            value = TargetStation.PodId;
            Log.Instance.Info(new LogMessage(transactionID, $"Reader {ReadID} returned Pod Id {value} at {TargetStation.StationId}"));
        }
        return value;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
