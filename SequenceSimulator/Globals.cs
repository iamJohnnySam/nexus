using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulatorSequence
{
    public static class Global
    {
        public static List<Thread> RunningThreads { get; set; } = [];
    }
}
