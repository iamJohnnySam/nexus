using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutSimulator.Helpers;

public sealed class InternalClock
{
    private static InternalClock? instance = null;

    private static readonly object padlock = new();

    public bool InternalTimeKeeper { get; set; } = false;
    public uint CurrentTime { get; private set; }
    public uint LastActionInternalTime { get; private set; }
    public DateTime LastActionTime { get; set; } = DateTime.Now;

    private InternalClock()
    {
        CurrentTime = 0;
        LastActionTime = DateTime.Now;
        LastActionInternalTime = 0;
    }

    public static InternalClock Instance
    {
        get
        {
            lock (padlock)
            {
                instance ??= new InternalClock();
                return instance;
            }
        }
    }

    public void Tick()
    {
        if (!InternalTimeKeeper)
            InternalTimeKeeper = true;
        CurrentTime++;
    }

    public void AdvanceTime(uint timeDelta)
    {
        if (!InternalTimeKeeper)
            InternalTimeKeeper = true;
        CurrentTime += timeDelta;
    }

    public void ProcessWait(uint waitTime)
    {
        uint actionEndTime = CurrentTime + waitTime;
        if (InternalTimeKeeper)
        {
            while (CurrentTime < actionEndTime)
            {
                Thread.Sleep(1);
            }
        }
        else
        {
            Thread.Sleep((int)(waitTime * 1000));
        }
        LastActionTime = DateTime.Now;
        LastActionInternalTime = CurrentTime;
    }
}
