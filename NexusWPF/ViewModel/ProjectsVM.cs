using NexusModels.Enums;
using NexusWPF.Commands.HomeCommands;
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
    public class ProjectsVM : ViewModelBase
    {
        private readonly IMainProjectManager projectManager;

        public ObservableCollection<Project> Projects => projectManager.Projects;

        public Module IntegrationProduct { get; set; }

        public string CurrentProjectName
        {
            get
            {
                if (projectManager.CurrentProject ==  null)
                    return string.Empty;
                return projectManager.CurrentProject.ProjectName;
            }

            set
            {
                if (projectManager.CurrentProject != null) 
                    projectManager.CurrentProject.ProjectName = value;
                OnPropertyChanged();
            }
        }
        public string CurrentProjectCustomer
        {
            get
            {
                if (projectManager.CurrentProject == null)
                    return string.Empty;
                return projectManager.CurrentProject.Customer;
            }

            set
            {
                if (projectManager.CurrentProject != null) 
                    projectManager.CurrentProject.Customer = value;
                OnPropertyChanged();
            }
        }
        public string CurrentProjectDesignCode
        {
            get
            {
                if (projectManager.CurrentProject == null)
                    return string.Empty;
                return projectManager.CurrentProject.DesignCode ?? string.Empty;
            }

            set
            {
                if (projectManager.CurrentProject != null) 
                    projectManager.CurrentProject.DesignCode = value;
                OnPropertyChanged();
            }
        }
        public string CurrentProjectSalesCode
        {
            get
            {
                if (projectManager.CurrentProject == null)
                    return string.Empty;
                return projectManager.CurrentProject.SalesCode ?? string.Empty;
            }

            set
            {
                if (projectManager.CurrentProject != null) 
                    projectManager.CurrentProject.SalesCode = value;
                OnPropertyChanged();
            }
        }
        public string CurrentProjectPOCode
        {
            get
            {
                if (projectManager.CurrentProject == null)
                    return string.Empty;
                return projectManager.CurrentProject.POCode ?? string.Empty;
            }

            set
            {
                if (projectManager.CurrentProject != null)
                    projectManager.CurrentProject.POCode = value;
                OnPropertyChanged();
            }
        }
        public ProductCategory CurrentProjectCategory
        {
            get
            {
                if (projectManager.CurrentProject == null)
                    return ProductCategory.None;
                return projectManager.CurrentProject.ProductCategory;
            }

            set
            {
                if (projectManager.CurrentProject != null)
                    projectManager.CurrentProject.ProductCategory = value;
                OnPropertyChanged();
            }
        }
        public ProjectPriority CurrentProjectPriority
        {
            get
            {
                if (projectManager.CurrentProject == null)
                    return ProjectPriority.NotStarted;
                return projectManager.CurrentProject.Priority;
            }

            set
            {
                if (projectManager.CurrentProject != null)
                    projectManager.CurrentProject.Priority = value;
                OnPropertyChanged();
            }
        }
        public SalesStatus CurrentProjectSalesStatus
        {
            get
            {
                if (projectManager.CurrentProject == null)
                    return SalesStatus.Concept;
                return projectManager.CurrentProject.POStatus;
            }

            set
            {
                if (projectManager.CurrentProject != null)
                    projectManager.CurrentProject.POStatus = value;
                OnPropertyChanged();
            }
        }
        public List<Module> CurrentProjectModules
        {
            get
            {
                if (projectManager.CurrentProject == null)
                    return [];
                return projectManager.CurrentProject.Modules;
            }

            set
            {
                if (projectManager.CurrentProject != null)
                    projectManager.CurrentProject.Modules = value;
                OnPropertyChanged();
            }
        }


        public bool SelectedProject => projectManager.SelectedProject;


        public ICommand SetCurrentProject { get; }
        public ICommand CreateNewProject { get; }
        public ICommand SaveProject { get; }
        public ICommand DeleteProject { get; }
        public ICommand AddModuleToProject { get; }



        public ProjectsVM(IMainProjectManager projectManager)
        {
            this.projectManager = projectManager;

            SetCurrentProject = new SetCurrentProjectCommand(projectManager);
            CreateNewProject = new CreateNewProjectCommand(projectManager);
            SaveProject = new SaveProjectCommand(projectManager);
            DeleteProject = new DeleteProjectCommand(projectManager);
            AddModuleToProject = new AddModuleToProjectCommand(this, projectManager);

            projectManager.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(projectManager.SelectedProject))
                    OnPropertyChanged(nameof(SelectedProject));

                if (e.PropertyName == nameof(projectManager.CurrentProject))
                {
                    OnPropertyChanged(nameof(CurrentProjectName));
                    OnPropertyChanged(nameof(CurrentProjectCustomer));
                    OnPropertyChanged(nameof(CurrentProjectDesignCode));
                    OnPropertyChanged(nameof(CurrentProjectSalesCode));
                    OnPropertyChanged(nameof(CurrentProjectPOCode));
                    OnPropertyChanged(nameof(CurrentProjectCategory));
                    OnPropertyChanged(nameof(CurrentProjectPriority));
                    OnPropertyChanged(nameof(CurrentProjectSalesStatus));
                    OnPropertyChanged(nameof(CurrentProjectModules));
                }
            };

            projectManager.UpdateProjects();
        }
    }
}
