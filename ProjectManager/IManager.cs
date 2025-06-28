using ProjectManager.Models;

namespace ProjectManager
{
	public interface IManager
	{
		Project CurrentProject { get; set; }
		List<Project> Projects { get; }
	}
}