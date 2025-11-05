using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutSimulator;

public enum ENAckCode
{
    SimulatorNotStarted,
    CommSpecError,
    CommandError,
    TargetNotExist,
    MissingArguments,
    Busy,
    NotDockable,
    NotMappable,
    StationDoesNotHaveDoor,
    PowerOff,
    EndEffectorMissing,
    ModuleNack
}
