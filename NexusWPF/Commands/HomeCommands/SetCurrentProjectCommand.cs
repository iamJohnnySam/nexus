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
    class SetCurrentProjectCommand : CommandBase
    {
        private IMainProjectManager _projectManager;
        public override void Execute(object? parameter)
        {
            if (parameter is Project project)
            {
                _projectManager.CurrentProject = project;
            }
        }

        public SetCurrentProjectCommand(IMainProjectManager projectManager)
        {
            _projectManager = projectManager;
        }
    }
}
