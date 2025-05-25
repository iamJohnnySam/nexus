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
    class DeleteProjectCommand : CommandBase
    {
        private IMainProjectManager _projectManager;
        public override void Execute(object? parameter)
        {
            _projectManager.DeleteCurrentProject();
        }

        public DeleteProjectCommand(IMainProjectManager projectManager)
        {
            _projectManager = projectManager;
        }
    }
}
