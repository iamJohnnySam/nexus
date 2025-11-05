using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutSimulator.Models;

public enum LayoutState
{
    ListeningCommands,
    Stopped,
    AutoRun,
    Paused,
}