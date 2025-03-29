using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
    public enum ErrorCodes
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
    public enum NAckCodes
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
        ModuleNAck
    }
}
