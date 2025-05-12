using NexusWPF.Services;
using NexusWPF.Utilities;
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
            _windowService.OpenWindow();
        }

        public OpenSimulatorCommand(IWindowService windowService)
        {
            _windowService = windowService;
        }
    }
}
