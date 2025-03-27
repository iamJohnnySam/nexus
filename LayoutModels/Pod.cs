using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutModels
{
    public class Pod(string podID, int capacity, string payloadType)
    {
        public Dictionary<int, Payload> slots = [];

        public string PodID { get; set; } = podID;
        public int Capacity { get; set; } = capacity;
        public string PayloadType { get; set; } = payloadType;
    }
}
