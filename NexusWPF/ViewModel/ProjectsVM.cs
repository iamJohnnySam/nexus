using NexusModels.Enums;
using NexusWPF.Commands.Home;
using ProjectManager;
using ProjectManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UIUtilities;

namespace NexusWPF.ViewModel
{
    class ProjectsVM : ViewModelBase
    {
        private readonly IMainProjectManager projectManager;

        public ObservableCollection<Project> Projects => projectManager.Projects;

        public string CurrentProjectName { get; set; }
        public string CurrentProjectCustomer {  get; set; }
        public string CurrentProjectDesignCode { get; set; }
        public string CurrentProjectPOCode { get; set; }
        public ProductCategory CurrentProjectCategory { get; set; }
        public ProjectPriority CurrentProjectPriority { get; set; }
        public SalesStatus CurrentProjectSalesStatus { get; set; }


        public ICommand SetCurrentProjectCommand { get; }

        public ProjectsVM(IMainProjectManager projectManager)
        {
            this.projectManager = projectManager;

            SetCurrentProjectCommand = new SetCurrentProjectCommand(projectManager);

            projectManager.UpdateProjects();
        }
    }
}
