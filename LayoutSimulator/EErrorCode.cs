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
    InvalidReservation,
    PodAlreadyAvailable,
    PodNotAvailable,
    PayloadAlreadyAvailable,
    PayloadNotAvailable,
    SlotsEmpty,
    SlotsNotEmpty,
    SlotOutOfBounds,
    SlotBlocked,
    NotAccessible,
    NotPodDockable,
    ActionWhileBusy,
    StationNotReachable,
    UnknownArmState,
    EndEffectorOutOfBounds,
    PayloadTypeMismatch,
    SlotIndexMissing,
    CassetteNotMovable,
    IncorrectState,
    ModuleError,
    TimedOut,
    MissingArguments,
}
