using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutSimulator.Commands;

public enum ECommandType
{
    Pick = 0,
    Place = 1,
    Door = 2,
    DoorOpen = 3,
    DoorClose = 4,
    Map = 5,
    Dock = 6,
    SDock = 7,
    Undock = 8,
    Process0 = 9,
    Process1 = 10,
    Process2 = 11,
    Process3 = 12,
    Process4 = 13,
    Process5 = 14,
    Process6 = 15,
    Process7 = 16,
    Process8 = 17,
    Process9 = 18,
    Power = 19,
    PowerOn = 20,
    PowerOff = 21,
    Home = 22,

    ReadSlot = 23,
    ReadPod = 24,

    Pod = 25,
    Payload = 26,

    StartSim = 28,
    StopSim = 29,
    PauseSim = 30,
    ResumeSim = 40
}
