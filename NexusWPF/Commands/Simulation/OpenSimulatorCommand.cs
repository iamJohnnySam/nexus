using NexusWPF.Services;
using NexusWPF.Utilities;
using NexusWPF.ViewModel.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusWPF.Commands.Simulation
{
    public class OpenSimulatorCommand : CommandBase
    {
        private IWindowService _windowService;

        public override void Execute(object? parameter)
        {
            _windowService.OpenWindow(new SequenceSimulationWindowVM("C:\\Users\\MT-0051\\source\\repos\\iamJohnnySam\\nexus\\SequenceSimulatorConsole\\layouts\\BeneqS3_3.xml", false));
        }

        public OpenSimulatorCommand(IWindowService windowService)
        {
            _windowService = windowService;
        }
    }
}
