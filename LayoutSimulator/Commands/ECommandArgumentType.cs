using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutSimulator.Commands;

public enum ECommandArgumentType
{
    EndEffector = 0,
    Slot = 1,
    TargetStation = 2,
    PodId = 3,
    DoorStatus = 4,
    PowerStatus = 5,

    Capacity = 6,
    Type = 7,

    Ignore = 8
}
