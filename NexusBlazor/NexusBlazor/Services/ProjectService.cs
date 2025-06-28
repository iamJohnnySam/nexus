using ProjectManager;
using ProjectManager.Models;

namespace NexusBlazor.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IMainProjectManager _manager;

        public ProjectService(IMainProjectManager manager)
        {
            _manager = manager;
        }

        public Task<IEnumerable<Project>> GetAllProjectsAsync()
        {
            throw new NotImplementedException();
        }

        public Task AddProjectAsync(Project project)
        {
            throw new NotImplementedException();
        }
    }
}
