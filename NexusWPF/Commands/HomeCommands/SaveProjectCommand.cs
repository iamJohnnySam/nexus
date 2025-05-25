using NexusWPF.Utilities;
using ProjectManager;
using ProjectManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusWPF.Commands.HomeCommands
{
    class SaveProjectCommand : CommandBase
    {
        private IMainProjectManager _projectManager;
        public override void Execute(object? parameter)
        {
            _projectManager.SaveCurrentProject();
        }

        public SaveProjectCommand(IMainProjectManager projectManager)
        {
            _projectManager = projectManager;
        }
    }
}
