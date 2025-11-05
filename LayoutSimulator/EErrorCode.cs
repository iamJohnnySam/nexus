using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutSimulator;

public enum EErrorCode
{
    SimulatorStopped,
    ProgramError,
    PodAlreadyAvailable,
    PodNotAvailable,
    PayloadAlreadyAvailable,
    PayloadNotAvailable,
    SlotsEmpty,
    SlotsNotEmpty,
    NotAccessible,
    PowerOffWhileBusy,
    StationNotReachable,
    UnknownArmState,
    PayloadTypeMismatch,
    SlotIndexMissing,
    IncorrectState,
    ModuleError,
    TimedOut,
    MissingArguments
}
