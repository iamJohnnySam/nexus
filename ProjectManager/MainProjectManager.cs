using Microsoft.EntityFrameworkCore;
using ProjectManager.DB;
using ProjectManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager
{
    public class MainProjectManager : IMainProjectManager
    {
        public Project? CurrentProject { get; set; }
        public ObservableCollection<Project> Projects { get; set; }

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

        public void UpdateProjects()
        {
            using var context = new AppDbContext();
            Projects = new ObservableCollection<Project>(context.Projects.ToList());
        }
    }
}
