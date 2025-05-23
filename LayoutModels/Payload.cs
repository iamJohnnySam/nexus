﻿using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutModels
{
    public class Payload
    {
        public event EventHandler<(string? tID, string message)>? OnLogEvent;
        public event EventHandler<string>? OnPayloadStateChange;

        public Payload(string payloadID, string payloadType, string lotID, int startingSlot)
        {
            PayloadID = payloadID;
            PayloadType = payloadType;
            LotID = lotID;
            StartingSlot = startingSlot;
        }

        public string PayloadID { get; set; }
        public string PayloadType { get; private set; }
        public string LotID { get; set; }
        public int StartingSlot { get; set; }

        private bool payloadErrorStaus = false;
        public bool PayloadErrorStaus {
            get { return payloadErrorStaus; }
            set {
                payloadErrorStaus = value;
                OnLogEvent?.Invoke(this, (null, $"Payload {PayloadID} error state updated to {value}"));
            }
        }

        private string payloadState = "unprocessed";
        public string PayloadState { 
            get { return payloadState; }
            set {
                payloadState = value;
                OnPayloadStateChange?.Invoke(this, value);
                OnLogEvent?.Invoke(this, (null, $"Payload {PayloadID} state updated to {value}"));
            }
        }

    }
}
