using Microsoft.EntityFrameworkCore;
using ProjectManager.DB;
using ProjectManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIUtilities;

namespace ProjectManager
{
    public class MainProjectManager : ViewModelBase, IMainProjectManager
    {
        private Project? _currentProject;
        public Project? CurrentProject
        {
            get
            {
                return _currentProject;
            }
            set
            {
                _currentProject = value;
                OnPropertyChanged();
                if (value != null)
                {
                    SelectedProject = true;
                }
                else
                {
                    SelectedProject = false;
                }
                currentProjectIsNew = false;
            }
        }


        private bool _selectedProject;
        public bool SelectedProject
        {
            get
            {
                return _selectedProject;
            }
            private set
            {
                _selectedProject = value;
                OnPropertyChanged();
            }
        }

        private bool currentProjectIsNew = false;

        public ObservableCollection<Project> Projects { get; set; } = [];


        public MainProjectManager()
        {
            DatabaseInitializer.EnsureDatabaseCreated();
            CreateGeneralProject();
        }

        private void CreateGeneralProject()
        {
            using var context = new AppDbContext();

            // Check if a project with the name "General" exists (case-insensitive)
            bool exists = context.Projects.Any(p => EF.Functions.Like(p.ProjectName, "General"));

            if (!exists)
            {
                var generalProject = new Project
                {
                    ProjectName = "General",
                    DesignCode = "GENERAL",
                    SalesCode = "GENERAL",
                    POCode = "GENERAL"
                };

                context.Projects.Add(generalProject);
                context.SaveChanges();
            }
        }
        public void CreateNewProject()
        {
            CurrentProject = new Project
            {
                ProjectName = "NEW PROJECT"
            };
            currentProjectIsNew = true;
        }

        public void UpdateProjects()
        {
            using var context = new AppDbContext();
            Projects = new ObservableCollection<Project>(context.Projects.ToList());
        }

        public void SaveCurrentProject()
        {
            if (CurrentProject == null)
            {
                return;
            }

            using var context = new AppDbContext();

            if (currentProjectIsNew)
            {
                context.Projects.Add(CurrentProject);
            }
            else
            {
                context.Projects.Update(CurrentProject); // Optional, EF tracks changes
            }

            context.SaveChanges();
            UpdateProjects();
        }
        public void DeleteCurrentProject()
        {
            using var context = new AppDbContext();

            if (CurrentProject != null)
            {
                context.Projects.Remove(CurrentProject);
                context.SaveChanges();
                SelectedProject = false;
            }
        }
    }
}
