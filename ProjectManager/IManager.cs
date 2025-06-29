using ProjectManager.Models;
using System.Data.SQLite;

namespace ProjectManager
{
    public interface IManager
    {
        SQLiteConnection Connection { get; }
        Project CurrentProject { get; set; }
        List<Project> Projects { get; }

        Project GetNewProject();
        void DeleteProject(int projectId);
        Task<List<Customer>> GetAllCustomers();
        Task<List<Product>> GetAllProducts();
        Task<List<Project>> GetAllProjects();
        Task<Project?> GetProject(int projectId);
        Task InsertCustomer(Customer p);
        Task InsertProduct(Product p);
        Task InsertProject(Project p);
        void UpdateProject(Project p);
        Task InsertTask(TaskItem p);
        Task<List<TaskItem>> GetAllTasks_L1();
    }
}