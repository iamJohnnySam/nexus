using ProjectManager;
using ProjectManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIUtilities;

namespace NexusWPF.ViewModel
{
    class HomeVM : ViewModelBase
    {
        private readonly IMainProjectManager projectManager;
        public ObservableCollection<Project> Projects => projectManager.Projects;

        public HomeVM(IMainProjectManager projectManager)
        {
            this.projectManager = projectManager;
            projectManager.UpdateProjects();
        }
    }
}
