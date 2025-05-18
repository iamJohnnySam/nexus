using ProjectManager.Models;
using System.Collections.ObjectModel;

namespace ProjectManager
{
    public interface IMainProjectManager
    {
        Project? CurrentProject { get; set; }
        ObservableCollection<Project> Projects { get; set; }

        public void UpdateProjects();
    }
}