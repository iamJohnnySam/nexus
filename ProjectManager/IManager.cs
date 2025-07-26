using ProjectManager.Models;
using System.Data.SQLite;

namespace ProjectManager
{
    public interface IManager
    {
        SQLiteConnection Connection { get; }
        Project CurrentProject { get; set; }
        List<Project> Projects { get; }

        Task DeleteCustomer(Customer customer);
        Task DeleteDeliverable(Deliverables d);
        Task DeleteDesignation(Designation d);
        Task DeleteEmployee(Employee emp);
        Task DeleteGrade(Grade g);
        Task DeleteProduct(Product p);
        Task<List<SimulationScenario>> GetSimulationScenarioByProjectId(int projectId);
        string CompileProjectName(Project project);
        Task DeleteProductModule(ProductModule module);
        Task<Deliverables?> GetDeliverableByProject(Project project);
        Task DeleteProject(Project p);
        Task DeleteReviewItem(ReviewItem item);
        Task DeleteReviewPoint(ReviewPoint point);
        Task DeleteSimulationScenario(SimulationScenario scenario);
        Task<List<Employee>> GetAllActiveEmployees();
        Task<List<string>> GetAllActiveProjectNames();
        Task<List<Project>> GetAllActiveProjects();
        Task<List<TaskItem>> GetAllCompleteParentTasks(int projectID = 0);
        Task<Dictionary<int, List<TaskItem>>> GetAllCompleteSubTasks(int projectID = 0);
        Task<List<Customer>> GetAllCustomers();
        Task<List<Deliverables>> GetAllDeliverables();
        Task<List<Designation>> GetAllDesignations();
        Task<List<Employee>> GetAllEmployees();
        Task<List<Grade>> GetAllGrades();
        Task<List<TaskItem>> GetAllIncompleteParentTasks(int projectID = 0);
        Task<Dictionary<int, List<TaskItem>>> GetAllIncompleteSubTasks(int projectID = 0);
        Task<List<TaskItem>> GetAllParentTasks(int projectID = 0);
        Task<List<ProductModule>> GetAllProductModules();
        Task<List<Product>> GetAllProducts();
        Task<List<string>> GetAllProjectNames();
        Task<List<Project>> GetAllProjects();
        Task<List<ReviewItem>> GetAllReviewItems();
        Task<List<ReviewPoint>> GetAllReviewPoints();
        Task<List<SimulationScenario>> GetAllSimulationScenarios();
        Task<Dictionary<int, List<TaskItem>>> GetAllSubTasks(int projectID = 0);
        Task<Customer?> GetCustomerById(int id);
        Task<Deliverables?> GetDeliverableById(int id);
        Task<Designation?> GetDesignationById(int id);
        Task<Employee?> GetEmployeeById(int id);
        Task<Grade?> GetGradeById(int id);
        TaskItem GetNewParentTask();
        Project GetNewProject();
        TaskItem GetNewSubTask(TaskItem p);
        Task<Product?> GetProductById(int id);
        Task<ProductModule?> GetProductModuleById(int id);
        Task<Project?> GetProject(int projectId);
        Task<ReviewItem?> GetReviewItemById(int id);
        Task<ReviewPoint?> GetReviewPointById(int id);
        Task<SimulationScenario?> GetSimulationScenarioById(int id);
        Task InsertCustomer(Customer customer);
        Task InsertDeliverable(Deliverables d);
        Task InsertDesignation(Designation d);
        Task InsertEmployee(Employee emp);
        Task InsertGrade(Grade g);
        Task InsertProduct(Product p);
        Task InsertProductModule(ProductModule module);
        Task InsertProject(Project p);
        Task InsertReviewItem(ReviewItem item);
        Task InsertReviewPoint(ReviewPoint point);
        Task InsertSimulationScenario(SimulationScenario scenario);
        Task InsertTask(TaskItem p);
        Task MarkTaskComplete(TaskItem p);
        Task MarkTaskIncomplete(TaskItem p);
        Task SelectProjectFromName(string projectName);
        Task UpdateCustomer(Customer customer);
        Task UpdateDeliverable(Deliverables d);
        Task UpdateDesignation(Designation d);
        Task UpdateEmployee(Employee emp);
        Task UpdateGrade(Grade g);
        Task UpdateProduct(Product p);
        Task UpdateProductModule(ProductModule module);
        Task UpdateProject(Project p);
        Task UpdateReviewItem(ReviewItem item);
        Task UpdateReviewPoint(ReviewPoint point);
        Task UpdateSimulationScenario(SimulationScenario scenario);
        Task UpdateTask(TaskItem p);
    }
}