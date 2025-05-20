using ProjectManager.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ProjectManager
{
    public interface IMainProjectManager
    {
        public event PropertyChangedEventHandler PropertyChanged;
        Project? CurrentProject { get; set; }
        bool SelectedProject { get; }
        ObservableCollection<Project> Projects { get; set; }

        public void CreateNewProject();
        public void DeleteCurrentProject();
        public void UpdateProjects();
        public void SaveCurrentProject();


    }
}