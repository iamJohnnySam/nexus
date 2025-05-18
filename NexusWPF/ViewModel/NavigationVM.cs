using NexusModels.Enums;
using NexusWPF.Utilities;
using ProjectManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Input;
using UIUtilities;

namespace NexusWPF.ViewModel
{
    public class NavigationVM : ViewModelBase
    {
        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }

        private string _pageTitle;
        private readonly IMainProjectManager projectManager;

        public string PageTitle
        {
            get { return _pageTitle; }
            set { _pageTitle = value.ToUpper(); OnPropertyChanged(); }
        }

        public string CurrentProject
        {
            get 
            {
                if (projectManager.CurrentProject == null)
                    return "Not Selected";
                return projectManager.CurrentProject.POStatus == SalesStatus.Concept ? projectManager.CurrentProject.SalesCode : projectManager.CurrentProject.DesignCode;
            }
        }

        public ICommand HomeCommand { get; set; }
        public ICommand ProjectsCommand { get; set; }
        public ICommand DocumentationCommand { get; set; }
        public ICommand FATCommand { get; set; }
        public ICommand TasksCommand { get; set; }
        public ICommand SimulationCommand { get; set; }


        private void Home(object obj)
        {
            CurrentView = new HomeVM(projectManager);
            PageTitle = string.Empty;
        }
        private void Projects(object obj)
        {
            CurrentView = new ProjectsVM();
            PageTitle = "Projects";
        }
        private void Documentation(object obj)
        {
            CurrentView = new DocumentationVM();
            PageTitle = "Documentation";
        }
        private void FAT(object obj)
        {
            CurrentView = new FATVM();
            PageTitle = "FAT Documentation";
        }
        private void Tasks(object obj)
        {
            CurrentView = new TasksVM();
            PageTitle = "Tasks";
        }
        private void Simulation(object obj)
        {
            CurrentView = new SimulationVM();
            PageTitle = "Sequence Simulation";
        }

        public NavigationVM(IMainProjectManager projectManager)
        {
            this.projectManager = projectManager;

            HomeCommand = new RelayCommand(Home);
            ProjectsCommand = new RelayCommand(Projects);
            DocumentationCommand = new RelayCommand(Documentation);
            FATCommand = new RelayCommand(FAT);
            TasksCommand = new RelayCommand(Tasks);
            SimulationCommand = new RelayCommand(Simulation);


            projectManager.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(projectManager.CurrentProject))
                    OnPropertyChanged(nameof(CurrentProject));
            };

            // Startup Page
            CurrentView = new HomeVM(projectManager);
            PageTitle = string.Empty;
        }
    }
}
