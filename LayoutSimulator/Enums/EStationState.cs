using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutSimulator.Enums;

public enum EStationState
{
    Off,
    Idle,
    UnDocked,
    Opening,
    Closing,
    Mapping,
    WaitingToBeAccessed,
    BeingAccessed,
    Processing
}
