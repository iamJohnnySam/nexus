using ProjectManager.Models;

namespace NexusBlazor.Services
{
    public interface IProjectService
    {
        Task AddProjectAsync(Project project);
        Task<IEnumerable<Project>> GetAllProjectsAsync();
    }
}