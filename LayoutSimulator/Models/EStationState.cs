using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutSimulator.Models;

public enum EStationState
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
