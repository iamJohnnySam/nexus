using LayoutModels.Stations;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutModels.Readers
{
    public class Reader : BaseStation, ITarget
    {
        // PROPERTIES
        public Station TargetStation { get; set; }
        public int SlotID { get; set; }
        private bool ReadSlot { get; set; }

        // CONSTRUCTORS
        public Reader(string readerID, string readerType, Station targetStation, int slot) : base(readerID, readerType)
        {
            StationID = readerID;
            TargetStation = targetStation;
            SlotID = slot;
            ReadSlot = true;

            targetStation.PairReader(readerID, slot);
        }
        public Reader(string readerID, string readerType, Station targetStation) : base(readerID, readerType)
        {
            StationID = readerID;
            TargetStation = targetStation;
            SlotID = 1;

            if (!TargetStation.PodDockable)
                ReadSlot = true;
            else
                ReadSlot = false;

            targetStation.PairReader(readerID);
        }

        // COMMANDS
        public string ReadID(string transactionID)
        {
            string value;
            if (ReadSlot)
            {
                if (TargetStation.Slots.TryGetValue(SlotID, out Payload? payload))
                {
                    value = payload.PayloadID;
                    Log(transactionID, $"Reader {ReadID} returned slot ID {value} at {TargetStation.StationID}");
                }
                else
                    throw new ErrorResponse(ErrorCodes.PayloadNotAvailable, $"Reader {StationID} did not have any payload on {TargetStation.StationID} slot {SlotID} to read.");
            }
            else
            {
                value = TargetStation.PodID ?? string.Empty;
                Log(transactionID, $"Reader {ReadID} returned Pod ID {value} at {TargetStation.StationID}");
            }
            return value;
        }
    }
}
