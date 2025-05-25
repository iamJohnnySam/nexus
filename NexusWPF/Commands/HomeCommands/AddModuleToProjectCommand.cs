using NexusWPF.Utilities;
using NexusWPF.ViewModel;
using ProjectManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusWPF.Commands.HomeCommands
{
    public class AddModuleToProjectCommand : CommandBase
    {
        private readonly IMainProjectManager projectManager;
        private readonly ProjectsVM projectsVM;

        public override void Execute(object? parameter)
        {
            if(projectManager.CurrentProject != null)
            {
                projectManager.CurrentProject.Modules.Add(projectsVM.IntegrationProduct);
                projectsVM.OnPropertyChanged(nameof(projectsVM.CurrentProjectModules));
            }
        }

        public AddModuleToProjectCommand(ProjectsVM projectVM, IMainProjectManager projectManager)
        {
            this.projectManager = projectManager;
            this.projectsVM = projectVM;
        }
    }
}
