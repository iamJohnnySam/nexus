using ProjectManager.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ProjectManager
{
    public interface IMainProjectManager
    {
        public event PropertyChangedEventHandler PropertyChanged;
        Project? CurrentProject { get; set; }
        ObservableCollection<Project> Projects { get; set; }

        public void UpdateProjects();


    }
}