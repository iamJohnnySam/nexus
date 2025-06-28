using NexusModels;
using ProjectManager.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ProjectManager
{
    public interface IMainProjectManager
    {
        Project CurrentProject { get; set; }
        ObservableCollection<Project> Projects { get; set; }

        public void CreateNewProject(Product product);
        public void DeleteCurrentProject();
        public void UpdateProjects();
        public void SaveCurrentProject();


    }
}