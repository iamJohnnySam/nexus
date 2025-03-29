using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutModels.Stations
{
    public enum StationState
    {
        Idle,
        Off,
        UnDocked,
        Opening,
        Closing,
        Mapping,
        BeingAccessed,
        Processing,
        Extending,
        Retracting,
        Moving,
    }
}
