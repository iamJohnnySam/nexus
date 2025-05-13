using NexusWPF.Commands.Simulation;
using NexusWPF.Services;
using NexusWPF.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UIUtilities;

namespace NexusWPF.ViewModel
{
    class SimulationVM : ViewModelBase
    {
		private string _selectedSimulation = "No Layout Selected";
        private IWindowService _simulatorWindowService;

		public string SelectedSimulation
		{
			get { return _selectedSimulation; }
			set { _selectedSimulation = value; OnPropertyChanged(); }
		}


        public ICommand StartSimulation { get; }

        public SimulationVM()
        {
            _simulatorWindowService = new SimulatorWindowService();
            StartSimulation = new OpenSimulatorCommand(_simulatorWindowService);
        }


    }
}
