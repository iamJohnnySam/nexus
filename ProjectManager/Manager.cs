using Dapper;
using ProjectManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ProjectManager
{
    public class Manager : IManager
    {
        private readonly string dbPath = "NexusDB.sqlite";
        private readonly string _connectionString;

        public SQLiteConnection Connection
        {
            get { return new SQLiteConnection(_connectionString); }
        }

        private Project currentProject;
        public Project CurrentProject
        {
            get { return currentProject; }
            set { currentProject = value; }
        }

        public List<Project> Projects
        {
            get
            {
                using var connection = new SQLiteConnection(_connectionString);
                connection.Open();
                return GetAllProjects().Result;
            }
        }


        public Manager()
        {
            _connectionString = $"Data Source={dbPath};Version=3;";

            bool databaseNotExist = false;
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
                databaseNotExist = true;
            }

            if (databaseNotExist)
            {
                using var connection = new SQLiteConnection(_connectionString);
                connection.Open();
                connection.Execute(tableCreationString);

                Customer newCustomer = new() { CustomerName = "Internal" };
                InsertCustomer(newCustomer).Wait();

                Product newProduct = new() { ProductName = "None" };
                InsertProduct(newProduct).Wait();

                InsertProject(new Project
                {
                    ProjectName = "General",
                    CustomerId = newCustomer.CustomerId,
                    DesignCode = "GENERAL",
                    Priority = ProjectPriority.Normal,
                    POStatus = SalesStatus.Concept,
                    ProductId = newProduct.ProductId,
                }).Wait();
            }
            currentProject = GetProject(1).Result!;
        }

        // ---- CUSTOMER DB
        public async Task InsertCustomer(Customer customer)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"INSERT INTO Customer (CustomerName)
                VALUES (@CustomerName);
                SELECT last_insert_rowid();";

            var id = await conn.ExecuteScalarAsync<long>(sql, customer);
            customer.CustomerId = (int)id;
        }
        public async Task<List<Customer>> GetAllCustomers()
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM Customer";
            var result = await conn.QueryAsync<Customer>(sql);
            return result.ToList();
        }
        public async Task<Customer?> GetCustomerById(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM Customer WHERE CustomerId = @id";
            return await conn.QueryFirstOrDefaultAsync<Customer>(sql, new { id });
        }
        public async Task UpdateCustomer(Customer customer)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"UPDATE Customer
                SET CustomerName = @CustomerName
                WHERE CustomerId = @CustomerId";

            await conn.ExecuteAsync(sql, customer);
        }
        public async Task DeleteCustomer(Customer customer)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "DELETE FROM Customer WHERE CustomerId = @CustomerId";
            await conn.ExecuteAsync(sql, customer);
        }


        // ---- PRODUCT DB
        public async Task InsertProduct(Product p)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"INSERT INTO Product (ProductName)
                VALUES (@ProductName);
                SELECT last_insert_rowid();";

            var id = await conn.ExecuteScalarAsync<long>(sql, p);
            p.ProductId = (int)id;
        }
        public async Task<List<Product>> GetAllProducts()
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM Product";
            var result = await conn.QueryAsync<Product>(sql);
            return result.ToList();
        }
        public async Task<Product?> GetProductById(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM Product WHERE ProductId = @id";
            return await conn.QueryFirstOrDefaultAsync<Product>(sql, new { id });
        }
        public async Task UpdateProduct(Product p)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"UPDATE Product
                SET ProductName = @ProductName
                WHERE ProductId = @ProductId";

            await conn.ExecuteAsync(sql, p);
        }
        public async Task DeleteProduct(Product p)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "DELETE FROM Product WHERE ProductId = @ProductId";
            await conn.ExecuteAsync(sql, p);
        }


        // ---- PROJECT DB
        public Project GetNewProject()
        {
            return new Project { ProjectName = "Untitled Project" };
        }
        public async Task InsertProject(Project p)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            string sql = @"INSERT INTO Project (ProjectName, CustomerId, DesignCode, Priority, POStatus, ProductId)
                       VALUES (@ProjectName, @CustomerId, @DesignCode, @Priority, @POStatus, @ProductId);";
            await conn.ExecuteAsync(sql, p);
            p.ProjectId = (int)conn.LastInsertRowId;
        }
        public async Task<Project?> GetProject(int projectId)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            string sql = @"
                SELECT p.*, c.CustomerId, c.CustomerName, pr.ProductId, pr.ProductName
                FROM Project p
                LEFT JOIN Customer c ON p.CustomerId = c.CustomerId
                LEFT JOIN Product pr ON p.ProductId = pr.ProductId
                WHERE p.ProjectId = @projectId;
                ";

            var result = await conn.QueryAsync<Project, Customer, Product, Project>(
                sql,
                (project, customer, product) =>
                {
                    project.Customer = customer;
                    project.Product = product;
                    return project;
                },
                new { projectId },
                splitOn: "CustomerId,ProductId"
            );
            return result.FirstOrDefault();
        }
        public async Task<List<Project>> GetAllProjects()
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            string sql = @"
                SELECT p.*, 
                       c.CustomerId, c.CustomerName, 
                       pr.ProductId, pr.ProductName
                FROM Project p
                LEFT JOIN Customer c ON p.CustomerId = c.CustomerId
                LEFT JOIN Product pr ON p.ProductId = pr.ProductId;
                ";

            var projects = await conn.QueryAsync<Project, Customer, Product, Project>(
                sql,
                (project, customer, product) =>
                {
                    project.Customer = customer;
                    project.Product = product;
                    return project;
                },
                splitOn: "CustomerId,ProductId"
            );
            return projects.ToList();
        }
        public async Task<List<string>> GetAllProjectNames()
        {
            return (await GetAllProjects()).Select(item => item?.GetType().GetProperty("ProjectName")?.GetValue(item)?.ToString())
                .Where(projectName => projectName != null)
                .ToList();
        }
        public async Task SelectProjectFromName(string projectName)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            string sql = @"
                SELECT p.*, c.CustomerId, c.CustomerName, pr.ProductId, pr.ProductName
                FROM Project p
                LEFT JOIN Customer c ON p.CustomerId = c.CustomerId
                LEFT JOIN Product pr ON p.ProductId = pr.ProductId
                WHERE p.ProjectName = @projectName;
                ";

            var result = await conn.QueryAsync<Project, Customer, Product, Project>(
                sql,
                (project, customer, product) =>
                {
                    project.Customer = customer;
                    project.Product = product;
                    return project;
                },
                new { projectName },
                splitOn: "CustomerId,ProductId"
            );
            Project? project = result.FirstOrDefault();
            if (project != null)
                currentProject = project;
        }
        public async Task UpdateProject(Project p)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            string sql = @"UPDATE Project
                       SET ProjectName = @ProjectName,
                           CustomerID = @CustomerID,
                           DesignCode = @DesignCode,
                           Priority = @Priority,
                           POStatus = @POStatus,
                           ProductId = @ProductId
                       WHERE ProjectId = @ProjectId;";
            await conn.ExecuteAsync(sql, p);
        }
        public async Task DeleteProject(int projectId)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            string sql = "DELETE FROM Project WHERE ProjectId = @ProjectId;";
            await conn.ExecuteAsync(sql, new { ProjectId = projectId });
        }


        // ---- TASK DB
        public TaskItem GetNewParentTask()
        {
            return new TaskItem
            {
                Title = "Untitled Task",
                ProjectId = currentProject.ProjectId,
                Deadline = DateTime.Now,
            };
        }
        public TaskItem GetNewSubTask(TaskItem p)
        {
            return new TaskItem
            {
                Title = "Untitled Task",
                ProjectId = currentProject.ProjectId,
                Deadline = DateTime.Now,
                ParentTaskId = p.TaskId
            };
        }
        public async Task InsertTask(TaskItem p)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            if (p.ProjectId == 0)
                p.ProjectId = currentProject.ProjectId;

            string sql = @"INSERT INTO TaskItem (ProjectId, Title, Description, CreatedOn, StartedOn, Deadline, IsCompleted, ParentTaskId)
                       VALUES (@ProjectId, @Title, @Description, @CreatedOn, @StartedOn, @Deadline, @IsCompleted, @ParentTaskId);";
            await conn.ExecuteAsync(sql, p);
            p.TaskId = (int)conn.LastInsertRowId;
        }
        private async Task<List<TaskItem>> QueryTaskList(string query, Project project)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            return (await conn.QueryAsync<TaskItem>(query, project)).ToList();
        }
        public async Task<List<TaskItem>> GetAllParentTasks()
        {
            string query = "SELECT * FROM TaskItem WHERE ProjectId = @ProjectId AND (ParentTaskId IS NULL OR ParentTaskId = 0)";
            return await QueryTaskList(query, currentProject);
        }
        public async Task<List<TaskItem>> GetAllIncompleteParentTasks()
        {
            string query = "SELECT * FROM TaskItem WHERE ProjectId = @ProjectId AND (ParentTaskId IS NULL OR ParentTaskId = 0) AND IsCompleted = 0";
            return await QueryTaskList(query, currentProject);
        }
        public async Task<List<TaskItem>> GetAllCompleteParentTasks()
        {
            string query = "SELECT * FROM TaskItem WHERE ProjectId = @ProjectId AND (ParentTaskId IS NULL OR ParentTaskId = 0) AND IsCompleted = 1";
            return await QueryTaskList(query, currentProject);
        }
        private Dictionary<int, List<TaskItem>> BundleSubTasks(List<TaskItem> AllTasks)
        {
            Dictionary<int, List<TaskItem>> SubTasks = [];
            foreach (TaskItem taskItem in AllTasks)
            {
                if (taskItem.ParentTaskId is null || taskItem.ParentTaskId == 0)
                    continue;

                if (!SubTasks.ContainsKey(taskItem.ParentTaskId ?? 0))
                {
                    SubTasks.Add(taskItem.ParentTaskId ?? 0, []);
                }

                SubTasks[taskItem.ParentTaskId ?? 0].Add(taskItem);
            }
            return SubTasks;
        }
        public async Task<Dictionary<int, List<TaskItem>>> GetAllSubTasks()
        {
            string query = "SELECT * FROM TaskItem WHERE ProjectId = @ProjectId AND ParentTaskId IS NOT NULL AND ParentTaskId <> 0";
            List<TaskItem> AllTasks = await QueryTaskList(query, currentProject);
            return BundleSubTasks(AllTasks);
        }
        public async Task<Dictionary<int, List<TaskItem>>> GetAllCompleteSubTasks()
        {
            string query = "SELECT * FROM TaskItem WHERE ProjectId = @ProjectId AND ParentTaskId IS NOT NULL AND ParentTaskId <> 0 AND IsCompleted = 1";
            List<TaskItem> AllTasks = await QueryTaskList(query, currentProject);
            return BundleSubTasks(AllTasks);
        }
        public async Task<Dictionary<int, List<TaskItem>>> GetAllIncompleteSubTasks()
        {
            string query = "SELECT * FROM TaskItem WHERE ProjectId = @ProjectId AND ParentTaskId IS NOT NULL AND ParentTaskId <> 0 AND IsCompleted = 0";
            List<TaskItem> AllTasks = await QueryTaskList(query, currentProject);
            return BundleSubTasks(AllTasks);
        }
        public async Task MarkTaskComplete(TaskItem p)
        {
            p.IsCompleted = true;
            await UpdateTaskCompletion(p);
        }
        public async Task MarkTaskIncomplete(TaskItem p)
        {
            p.IsCompleted = true;
            await UpdateTaskCompletion(p);
        }
        public async Task UpdateTask(TaskItem p)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            string sql = @"UPDATE TaskItem
                       SET ProjectId = @ProjectId,
                           Title = @Title,
                           Description = @Description,
                           CreatedOn = @CreatedOn,
                           StartedOn = @StartedOn,
                           Deadline = @Deadline,
                           IsCompleted = @IsCompleted,
                           ParentTaskId = @ParentTaskId
                       WHERE TaskId = @TaskId;";
            await conn.ExecuteAsync(sql, p);
        }
        private async Task UpdateTaskCompletion(TaskItem p)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            string sql = @"UPDATE TaskItem
                       SET IsCompleted = @IsCompleted,
                       WHERE TaskId = @TaskId;";
            await conn.ExecuteAsync(sql, p);
        }


        // ---- DESIGNATION DB
        public async Task InsertDesignation(Designation d)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"INSERT INTO Designation (DesignationName, Department)
                VALUES (@DesignationName, @Department);
                SELECT last_insert_rowid();";
            var id = await conn.ExecuteScalarAsync<long>(sql, d);
            d.DesignationId = (int)id;
        }
        public async Task<List<Designation>> GetAllDesignations()
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "SELECT * FROM Designation";
            var result = await conn.QueryAsync<Designation>(sql);
            return result.ToList();
        }
        public async Task<Designation?> GetDesignationById(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "SELECT * FROM Designation WHERE DesignationId = @id";
            return await conn.QueryFirstOrDefaultAsync<Designation>(sql, new { id });
        }
        public async Task UpdateDesignation(Designation d)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"UPDATE Designation 
                SET DesignationName = @DesignationName, 
                    Department = @Department
                WHERE DesignationId = @DesignationId";
            await conn.ExecuteAsync(sql, d);
        }
        public async Task DeleteDesignation(Designation d)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "DELETE FROM Designation WHERE DesignationId = @DesignationId";
            await conn.ExecuteAsync(sql, d);
        }


        // ---- GRADE DB
        public async Task InsertGrade(Grade g)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"INSERT INTO Grade (GradeName, GradeScore)
                VALUES (@GradeName, @GradeScore);
                SELECT last_insert_rowid();";
            var id = await conn.ExecuteScalarAsync<long>(sql, g);
            g.GradeId = (int)id;
        }
        public async Task<List<Grade>> GetAllGrades()
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "SELECT * FROM Grade";
            var result = await conn.QueryAsync<Grade>(sql);
            return result.ToList();
        }
        public async Task<Grade?> GetGradeById(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "SELECT * FROM Grade WHERE GradeId = @id";
            return await conn.QueryFirstOrDefaultAsync<Grade>(sql, new { id });
        }
        public async Task UpdateGrade(Grade g)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"UPDATE Grade 
                SET GradeName = @GradeName, 
                    GradeScore = @GradeScore
                WHERE GradeId = @GradeId";
            await conn.ExecuteAsync(sql, g);
        }
        public async Task DeleteGrade(Grade g)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "DELETE FROM Grade WHERE GradeId = @GradeId";
            await conn.ExecuteAsync(sql, g);
        }


        // ---- EMPLOYEE DB
        public async Task InsertEmployee(Employee emp)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"INSERT INTO Employee 
        (EmployeeName, GradeId, DesignationId, JoinDate, LeaveDate, IsActive, ReplacedEmployeeId)
        VALUES (@EmployeeName, @GradeId, @DesignationId, @JoinDate, @LeaveDate, @IsActive, @ReplacedEmployeeId);
        SELECT last_insert_rowid();";

            var id = await conn.ExecuteScalarAsync<long>(sql, emp);
            emp.EmployeeId = (int)id;
        }
        public async Task<List<Employee>> GetAllEmployees()
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM Employee";
            var result = await conn.QueryAsync<Employee>(sql);
            return result.ToList();
        }
        public async Task<Employee?> GetEmployeeById(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM Employee WHERE EmployeeId = @id";
            return await conn.QueryFirstOrDefaultAsync<Employee>(sql, new { id });
        }
        public async Task UpdateEmployee(Employee emp)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"UPDATE Employee SET
        EmployeeName = @EmployeeName,
        GradeId = @GradeId,
        DesignationId = @DesignationId,
        JoinDate = @JoinDate,
        LeaveDate = @LeaveDate,
        IsActive = @IsActive,
        ReplacedEmployeeId = @ReplacedEmployeeId
        WHERE EmployeeId = @EmployeeId";

            await conn.ExecuteAsync(sql, emp);
        }
        public async Task DeleteEmployee(Employee emp)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "DELETE FROM Employee WHERE EmployeeId = @EmployeeId";
            await conn.ExecuteAsync(sql, emp);
        }


        // ---- PRODUCT MODULE DB
        public async Task InsertProductModule(ProductModule module)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"INSERT INTO Module (ModuleName)
                VALUES (@ModuleName);
                SELECT last_insert_rowid();";

            var id = await conn.ExecuteScalarAsync<long>(sql, module);
            module.ModuleId = (int)id;
        }
        public async Task<List<ProductModule>> GetAllProductModules()
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM Module";
            var result = await conn.QueryAsync<ProductModule>(sql);
            return result.ToList();
        }
        public async Task<ProductModule?> GetProductModuleById(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM Module WHERE ModuleId = @id";
            return await conn.QueryFirstOrDefaultAsync<ProductModule>(sql, new { id });
        }
        public async Task UpdateProductModule(ProductModule module)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"UPDATE Module
                SET ModuleName = @ModuleName
                WHERE ModuleId = @ModuleId";

            await conn.ExecuteAsync(sql, module);
        }
        public async Task DeleteProductModule(ProductModule module)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "DELETE FROM Module WHERE ModuleId = @ModuleId";
            await conn.ExecuteAsync(sql, module);
        }


        // ---- REVIEW ITEM DB
        public async Task InsertReviewItem(ReviewItem item)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"INSERT INTO ReviewItem 
        (ProjectId, ReviewPointId, Approved, LastReviewDate, ReviewComments, ReviewResponsibleID)
        VALUES (@ProjectId, @ReviewPointId, @Approved, @LastReviewDate, @ReviewComments, @ReviewResponsibleID);
        SELECT last_insert_rowid();";

            var id = await conn.ExecuteScalarAsync<long>(sql, item);
            item.ReviewItemId = (int)id;
        }
        public async Task<List<ReviewItem>> GetAllReviewItems()
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM ReviewItem";
            var result = await conn.QueryAsync<ReviewItem>(sql);
            return result.ToList();
        }
        public async Task<ReviewItem?> GetReviewItemById(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM ReviewItem WHERE ReviewItemId = @id";
            return await conn.QueryFirstOrDefaultAsync<ReviewItem>(sql, new { id });
        }
        public async Task UpdateReviewItem(ReviewItem item)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"UPDATE ReviewItem SET
        ProjectId = @ProjectId,
        ReviewPointId = @ReviewPointId,
        Approved = @Approved,
        LastReviewDate = @LastReviewDate,
        ReviewComments = @ReviewComments,
        ReviewResponsibleID = @ReviewResponsibleID
        WHERE ReviewItemId = @ReviewItemId";

            await conn.ExecuteAsync(sql, item);
        }
        public async Task DeleteReviewItem(ReviewItem item)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "DELETE FROM ReviewItem WHERE ReviewItemId = @ReviewItemId";
            await conn.ExecuteAsync(sql, item);
        }


        // ---- REVIEW POINT DB
        public async Task InsertReviewPoint(ReviewPoint point)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"INSERT INTO ReviewPoint (ModuleId, ReviewDescription)
                VALUES (@ModuleId, @ReviewDescription);
                SELECT last_insert_rowid();";

            var id = await conn.ExecuteScalarAsync<long>(sql, point);
            point.ReviewPointId = (int)id;
        }
        public async Task<List<ReviewPoint>> GetAllReviewPoints()
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM ReviewPoint";
            var result = await conn.QueryAsync<ReviewPoint>(sql);
            return result.ToList();
        }
        public async Task<ReviewPoint?> GetReviewPointById(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM ReviewPoint WHERE ReviewPointId = @id";
            return await conn.QueryFirstOrDefaultAsync<ReviewPoint>(sql, new { id });
        }
        public async Task UpdateReviewPoint(ReviewPoint point)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"UPDATE ReviewPoint SET
                ModuleId = @ModuleId,
                ReviewDescription = @ReviewDescription
                WHERE ReviewPointId = @ReviewPointId";

            await conn.ExecuteAsync(sql, point);
        }
        public async Task DeleteReviewPoint(ReviewPoint point)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "DELETE FROM ReviewPoint WHERE ReviewPointId = @ReviewPointId";
            await conn.ExecuteAsync(sql, point);
        }



        private static string tableCreationString = @"
            CREATE TABLE IF NOT EXISTS Customer (
                CustomerId INTEGER PRIMARY KEY,
                CustomerName TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Product (
                ProductId INTEGER PRIMARY KEY,
                ProductName TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Project (
                ProjectId INTEGER PRIMARY KEY,
                ProjectName TEXT NOT NULL,
                CustomerId INTEGER,
                DesignCode TEXT,
                Priority INTEGER,
                POStatus INTEGER,
                ProductId INTEGER
            );
            
            CREATE TABLE IF NOT EXISTS ProductModule(
                ModuleId INTEGER PRIMARY KEY,
                ModuleName TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Product(
                ProductId INTEGER PRIMARY KEY,
                ProductName TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Grade(
                GradeId INTEGER PRIMARY KEY,
                GradeName TEXT NOT NULL,
                GradeScore INTEGER
            );
        
            CREATE TABLE IF NOT EXISTS Designation(
                DesignationId INTEGER PRIMARY KEY,
                DesignationName TEXT NOT NULL,
                Department TEXT
            );

            CREATE TABLE IF NOT EXISTS Employee(
                EmployeeId INTEGER PRIMARY KEY,
                EmployeeName TEXT NOT NULL,
                GradeId INTEGER,
                DesignationId INTEGER,
                JoinDate TEXT,
                LeaveDate TEXT,
                IsActive INTEGER,
                ReplacedEmployeeId INTEGER
            );

            CREATE TABLE IF NOT EXISTS ReviewPoint(
                ReviewPointId INTEGER PRIMARY KEY,
                ModuleId INTEGER,
                ReviewDescription TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS ReviewItem(
                ReviewItemId INTEGER PRIMARY KEY,
                ProjectId INTEGER,
                ReviewPointId INTEGER,
                Approved INTEGER,
                LastReviewDate TEXT,
                ReviewComments TEXT,
                ReviewResponsibleID INTEGER
            );

            CREATE TABLE IF NOT EXISTS TaskItem(
                TaskId INTEGER PRIMARY KEY,
                ProjectId INTEGER,
                Title TEXT NOT NULL,
                Description TEXT,
                CreatedOn TEXT,
                StartedOn TEXT,
                Deadline TEXT NOT NULL,
                IsCompleted INTEGER,
                ParentTaskId INTEGER
            );

            CREATE TABLE IF NOT EXISTS TaskItemResponsibility(
                TaskId INTEGER,
                EmployeeId INTEGER
            );

            CREATE TABLE IF NOT EXISTS PreviousProjectCodes(
                ProjectId INTEGER,
                Code TEXT
            );

            CREATE TABLE IF NOT EXISTS ProjectPCodes(
                ProjectId INTEGER,
                Code TEXT
            );";

    }
}
