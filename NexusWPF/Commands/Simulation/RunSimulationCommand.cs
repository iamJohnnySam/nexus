using NexusWPF.Utilities;
using NexusWPF.ViewModel.Simulation;
using SequenceSimulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusWPF.Commands.Simulation
{
    internal class RunSimulationCommand : CommandBase
    {
        private Simulator _simulator;
        private readonly SequenceSimulationWindowVM _windowVM;

        public override void Execute(object? parameter)
        {
            if (int.TryParse(_windowVM.RunTime, out int value))
            {
                _simulator.RunSimulatorThreaded(value);
            }
        }

        public override bool CanExecute(object? parameter)
        {
            return int.TryParse(_windowVM.RunTime, out _) && base.CanExecute(parameter);
        }

        public RunSimulationCommand(SequenceSimulationWindowVM windowVM, Simulator simulator)
        {
            _windowVM = windowVM;
            _simulator = simulator;

            _windowVM.PropertyChanged += _windowVM_PropertyChanged;
        }

        private void _windowVM_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_windowVM.RunTime))
            {
                OnCanExecuteChanged();
            }
        }
    }
}
