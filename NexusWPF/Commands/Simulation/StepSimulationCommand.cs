using NexusWPF.Utilities;
using SequenceSimulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusWPF.Commands.Simulation
{
    internal class StepSimulationCommand : CommandBase
    {
        private Simulator _simulator;

        public override void Execute(object? parameter)
        {
            _simulator.RunSimulator(1);
        }

        public StepSimulationCommand(Simulator simulator)
        {
            _simulator = simulator;
        }
    }
}
