using LayoutModels.Stations;
using NexusWPF.Commands.Simulation;
using NexusWPF.Utilities;
using SequenceSimulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UIUtilities;

namespace NexusWPF.ViewModel.Simulation
{
    public class SequenceSimulationWindowVM : ViewModelBase
    {
        private Simulator _simulator;


        public string TotalTime => _simulator.TotalTime.ToString();

        public IEnumerable<KeyValuePair<string, Station>> StationList => _simulator.layout.StationList;

        public ICommand StepSimulation { get; }
        public ICommand StartSimulation { get; }


        public SequenceSimulationWindowVM(string layoutFile, bool ignoreLotIDMatching)
        {
            _simulator = new();
            _simulator.InitializeSimulator(layoutFile, ignoreLotIDMatching);
            OnPropertyChanged(nameof(StationList));


            StepSimulation = new StepSimulationCommand(_simulator);


            _simulator.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_simulator.TotalTime))
                    OnPropertyChanged(nameof(TotalTime));
            };
        }
    }
}
    