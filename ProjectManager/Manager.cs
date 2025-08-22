using Dapper;
using ProjectManager.Models;
using ProjectManager.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;

namespace ProjectManager
{
    public class Manager
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

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            connection.Execute(CustomerTableCreationString);
            connection.Execute(ProjectTableCreationString);
            connection.Execute(TaskItemTableCreationString);
            connection.Execute(ProductModuleTableCreationString);
            connection.Execute(EmployeeTableCreationString);
            connection.Execute(SimulationScenarioTableCreationString);
            connection.Execute(ResourceBlockTableCreationString);
            connection.Execute(FunctionalKPITableCreationString);
            connection.Execute(ProjectStageTableCreationString);
            connection.Execute(ProjectBlockTableCreationString);
            connection.Execute(MilestoneTableCreationString);
            connection.Execute(SpecificationTableCreationString);
            connection.Execute(ConfigurationTableCreationString);
            connection.Execute(ConfigDetailTableCreationString);

            connection.Execute(tableCreationString);

            if (databaseNotExist)
            {
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
            currentProject = GetProjectById(1).Result;
        }


        // ---- CUSTOMER DB
        string CustomerTableCreationString = @"
                        CREATE TABLE IF NOT EXISTS Customer (
                            CustomerId INTEGER PRIMARY KEY,
                            CustomerName TEXT NOT NULL);";
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
        string ProjectTableCreationString = @"
                        CREATE TABLE IF NOT EXISTS Project (
                            ProjectId INTEGER PRIMARY KEY,
                            ProjectName TEXT NOT NULL,
                            CustomerId INTEGER,
                            DesignCode TEXT,
                            ProjectCode, TEXT,
                            Priority INTEGER,
                            POStatus INTEGER,
                            ProductId INTEGER,
                            IsActive INTEGER,
                            IsTrackedProject INTEGER,
                            PrimaryDesignerId INTEGER,
                            RequirementDocumentLink, TEXT);";
        public Project GetNewProject()
        {
            return new Project { ProjectName = "Untitled Project" };
        }
        public async Task InsertProject(Project p)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            string sql = @"INSERT INTO Project (ProjectName, CustomerId, DesignCode, ProjectCode, Priority, POStatus, ProductId, IsActive, IsTrackedProject, PrimaryDesignerId, RequirementDocumentLink)
                       VALUES (@ProjectName, @CustomerId, @DesignCode, @ProjectCode, @Priority, @POStatus, @ProductId, @IsActive, @IsTrackedProject, @PrimaryDesignerId, @RequirementDocumentLink);";
            await conn.ExecuteAsync(sql, p);
            p.ProjectId = (int)conn.LastInsertRowId;
        }
        public async Task GetProjectObjects(Project project)
        {
            project.Customer = await GetCustomerById(project.CustomerId);
            project.Product = await GetProductById(project.ProductId);
            project.PrimaryDesigner = await GetEmployeeById(project.PrimaryDesignerId);
        }
        public async Task<Project> GetProjectById(int projectId)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM Project WHERE ProjectId = @projectId";
            var project = await conn.QueryFirstOrDefaultAsync<Project>(sql, new { projectId });

            if (project != null)
                await GetProjectObjects(project);
            else
                throw new Exception($"Project with ID '{projectId}' not found.");

            return project;
        }
        public async Task<List<Project>> GetAllProjects()
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM Project ORDER BY ProjectName ASC;";
            var projects = (await conn.QueryAsync<Project>(sql)).ToList();

            foreach (var project in projects)
            {
                await GetProjectObjects(project);
            }

            return projects;
        }
        public async Task<List<Project>> GetAllActiveProjects()
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"SELECT * FROM Project 
                        WHERE IsActive = 1 
                        ORDER BY ProjectName ASC;";
            var projects = (await conn.QueryAsync<Project>(sql)).ToList();

            foreach (var project in projects)
            {
                await GetProjectObjects(project);
            }

            return projects;
        }
        public async Task<List<Project>> GetAllActiveTrackedProjects()
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"SELECT * FROM Project 
                        WHERE IsActive = 1 AND IsTrackedProject = 1
                        ORDER BY ProjectName ASC;";
            var projects = (await conn.QueryAsync<Project>(sql)).ToList();

            foreach (var project in projects)
            {
                await GetProjectObjects(project);
            }

            return projects;
        }
        public async Task<List<string>> GetAllProjectNames()
        {
            return (await GetAllProjects()).Select(item => item?.GetType().GetProperty("ProjectName")?.GetValue(item)?.ToString())
                .Where(projectName => projectName != null)
                .ToList();
        }
        public async Task<List<string>> GetAllActiveProjectNames()
        {
            return CompileProjectNames(await (GetAllActiveProjects()));
        }
        public async Task<List<string>> GetAllActiveTrackedProjectNames()
        {
            return CompileProjectNames(await (GetAllActiveTrackedProjects()));
        }
        private List<string> CompileProjectNames(List<Project> projects)
        {
            List<string> projectNames = [];
            foreach (Project project in projects)
            {
                projectNames.Add(CompileProjectName(project));
            }
            return projectNames;
        }
        public string CompileProjectName(Project project)
        {
            string name = string.Empty;
            if (project.DesignCode != null && project.DesignCode != string.Empty)
                name = $"{project.DesignCode} | ";
            name = name + project.ProjectName;
            return name;
        }
        public async Task<Project> SelectProjectFromName(string projectName)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM Project WHERE ProjectName = @projectName";
            var project = await conn.QueryFirstOrDefaultAsync<Project>(sql, new { projectName });

            if (project != null)
            {
                await GetProjectObjects(project);
                currentProject = project;
            }
            else
            {
                throw new Exception($"Project with name '{projectName}' not found.");
            }
            return project;
        }
        public async Task UpdateProject(Project p)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            string sql = @"UPDATE Project
                       SET ProjectName = @ProjectName,
                           CustomerId = @CustomerId,
                           DesignCode = @DesignCode,
                            ProjectCode = @ProjectCode, 
                            Priority = @Priority,
                            POStatus = @POStatus,
                            ProductId = @ProductId,
                            IsActive = @IsActive,
                            IsTrackedProject = @IsTrackedProject,
                            PrimaryDesignerId = @PrimaryDesignerId,
                            RequirementDocumentLink = @RequirementDocumentLink
                       WHERE ProjectId = @ProjectId;";
            await conn.ExecuteAsync(sql, p);
        }
        public async Task DeleteProject(Project p)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            string sql = "DELETE FROM Project WHERE ProjectId = @ProjectId;";
            await conn.ExecuteAsync(sql, new { ProjectId = p.ProjectId });
        }


        // ---- TASK DB
        string TaskItemTableCreationString = @"
                        CREATE TABLE IF NOT EXISTS TaskItem(
                            TaskId INTEGER PRIMARY KEY,
                            ProjectId INTEGER,
                            Title TEXT NOT NULL,
                            Description TEXT,
                            CreatedOn TEXT,
                            StartedOn TEXT,
                            Deadline TEXT NOT NULL,
                            ResponsibleId INTEGER,
                            IsCompleted INTEGER,
                            IsBlocking INTEGER DEFAULT 1,
                            ParentTaskId INTEGER,
                            PriorityTask INTEGER);";
        public TaskItem GetNewParentTask()
        {
            return new TaskItem
            {
                Title = "Untitled Task",
                ProjectId = currentProject.ProjectId,
                Deadline = DateTime.Now,
                ResponsibleId = 0
            };
        }
        public TaskItem GetNewSubTask(TaskItem t)
        {
            return new TaskItem
            {
                Title = "Untitled Task",
                ProjectId = currentProject.ProjectId,
                Deadline = DateTime.Now,
                ParentTaskId = t.TaskId,
                ResponsibleId = 0
            };
        }
        public async Task InsertTask(TaskItem t)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            string sql = @"INSERT INTO TaskItem (ProjectId, Title, Description, CreatedOn, StartedOn, Deadline,
                                         ResponsibleId, IsCompleted, IsBlocking, ParentTaskId, PriorityTask)
                   VALUES (@ProjectId, @Title, @Description, @CreatedOn, @StartedOn, @Deadline,
                           @ResponsibleId, @IsCompleted, @IsBlocking, @ParentTaskId, @PriorityTask);";
            await conn.ExecuteAsync(sql, t);
            t.TaskId = (int)conn.LastInsertRowId;
        }
        private async Task<List<TaskItem>> QueryTaskList(string query, object project)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            var tasks = (await conn.QueryAsync<TaskItem>(query, project)).ToList();

            foreach (var task in tasks)
            {
                task.Responsible = await conn.QueryFirstOrDefaultAsync<Employee>(
                    "SELECT * FROM Employee WHERE EmployeeId = @EmployeeId", new { EmployeeId = task.ResponsibleId });
            }

            return tasks;
        }
        public async Task<List<TaskItem>> GetAllParentTasks(int projectID = 0)
        {
            object reference = currentProject;
            if (projectID != 0)
            {
                reference = new { ProjectId = projectID };
            }

            string query = "SELECT * FROM TaskItem WHERE ProjectId = @ProjectId AND (ParentTaskId IS NULL OR ParentTaskId = 0)";
            return await QueryTaskList(query, reference);
        }
        public async Task<List<TaskItem>> GetAllIncompleteParentTasks(int projectID = 0)
        {
            object reference = currentProject;
            if (projectID != 0)
            {
                reference = new { ProjectId = projectID };
            }

            string query = "SELECT * FROM TaskItem WHERE ProjectId = @ProjectId AND (ParentTaskId IS NULL OR ParentTaskId = 0) AND IsCompleted = 0";
            return await QueryTaskList(query, reference);
        }
        public async Task<List<TaskItem>> GetAllCompleteParentTasks(int projectID = 0)
        {
            object reference = currentProject;
            if (projectID != 0)
            {
                reference = new { ProjectId = projectID };
            }

            string query = "SELECT * FROM TaskItem WHERE ProjectId = @ProjectId AND (ParentTaskId IS NULL OR ParentTaskId = 0) AND IsCompleted = 1";
            return await QueryTaskList(query, reference);
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
        public async Task<Dictionary<int, List<TaskItem>>> GetAllSubTasks(int projectID = 0)
        {
            object reference = currentProject;
            if (projectID != 0)
            {
                reference = new { ProjectId = projectID };
            }

            string query = "SELECT * FROM TaskItem WHERE ProjectId = @ProjectId AND ParentTaskId IS NOT NULL AND ParentTaskId <> 0";
            List<TaskItem> AllTasks = await QueryTaskList(query, reference);
            return BundleSubTasks(AllTasks);
        }
        public async Task<Dictionary<int, List<TaskItem>>> GetAllCompleteSubTasks(int projectID = 0)
        {
            object reference = currentProject;
            if (projectID != 0)
            {
                reference = new { ProjectId = projectID };
            }

            string query = "SELECT * FROM TaskItem WHERE ProjectId = @ProjectId AND ParentTaskId IS NOT NULL AND ParentTaskId <> 0 AND IsCompleted = 1";
            List<TaskItem> AllTasks = await QueryTaskList(query, reference);
            return BundleSubTasks(AllTasks);
        }
        public async Task<Dictionary<int, List<TaskItem>>> GetAllIncompleteSubTasks(int projectID = 0)
        {
            object reference = currentProject;
            if (projectID != 0)
            {
                reference = new { ProjectId = projectID };
            }

            string query = "SELECT * FROM TaskItem WHERE ProjectId = @ProjectId AND ParentTaskId IS NOT NULL AND ParentTaskId <> 0 AND IsCompleted = 0";
            List<TaskItem> AllTasks = await QueryTaskList(query, reference);
            return BundleSubTasks(AllTasks);
        }
        public async Task MarkTaskComplete(TaskItem t)
        {
            t.IsCompleted = true;
            await UpdateTaskCompletion(t);
        }
        public async Task MarkTaskIncomplete(TaskItem t)
        {
            t.IsCompleted = false;
            await UpdateTaskCompletion(t);
        }
        public async Task UpdateTask(TaskItem t)
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
                       ResponsibleId = @ResponsibleId,
                       IsCompleted = @IsCompleted,
                       IsBlocking = @IsBlocking,
                       ParentTaskId = @ParentTaskId,
                        PriorityTask = @PriorityTask
                   WHERE TaskId = @TaskId;";
            await conn.ExecuteAsync(sql, t);
        }
        private async Task UpdateTaskCompletion(TaskItem p)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            string sql = @"UPDATE TaskItem
                       SET IsCompleted = @IsCompleted
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
        string EmployeeTableCreationString = @"
                CREATE TABLE IF NOT EXISTS Employee(
                EmployeeId INTEGER PRIMARY KEY,
                EmployeeNumber TEXT,
                EmployeeName TEXT NOT NULL,
                GradeId INTEGER,
                DesignationId INTEGER,
                JoinDate TEXT,
                LeaveDate TEXT,
                IsActive INTEGER,
                ReplacedEmployeeId INTEGER);
";
        public async Task InsertEmployee(Employee emp)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
        INSERT INTO Employee (
            EmployeeNumber, EmployeeName, GradeId, DesignationId, JoinDate, LeaveDate, IsActive, ReplacedEmployeeId
        ) VALUES (
            @EmployeeNumber, @EmployeeName, @GradeId, @DesignationId, @JoinDate, @LeaveDate, @IsActive, @ReplacedEmployeeId
        );
        SELECT last_insert_rowid();";

            var id = await conn.ExecuteScalarAsync<long>(sql, new
            {
                emp.EmployeeName,
                emp.GradeId,
                emp.DesignationId,
                emp.JoinDate,
                emp.LeaveDate,
                emp.IsActive,
                emp.ReplacedEmployeeId
            });

            emp.EmployeeId = (int)id;
        }
        public async Task<List<Employee>> GetAllEmployees()
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var employees = (await conn.QueryAsync<Employee>("SELECT * FROM Employee")).ToList();

            foreach (var emp in employees)
            {
                emp.EmployeeGrade = await GetGradeById(emp.GradeId);
                emp.EmployeeDesignation = await GetDesignationById(emp.DesignationId);
            }

            return employees;
        }
        public async Task<List<Employee>> GetAllActiveEmployees()
        {
            FixActiveEmployees().Wait();
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var employees = (await conn.QueryAsync<Employee>("SELECT * FROM Employee WHERE IsActive = 1")).ToList();

            foreach (var emp in employees)
            {
                emp.EmployeeGrade = await GetGradeById(emp.GradeId);
                emp.EmployeeDesignation = await GetDesignationById(emp.DesignationId);
            }

            List<Employee> groupedAndSorted = employees
                .OrderBy(e => e.EmployeeDesignation.DesignationName)
                .ThenByDescending(e => e.EmployeeGrade.GradeScore)
                .ToList();

            return groupedAndSorted;
        }
        public async Task FixActiveEmployees()
        {
            List<Employee> employees = await GetAllEmployees();
            var today = DateTime.Today;

            foreach (var emp in employees)
            {
                bool status = false;
                if (emp.JoinDate > today || (emp.LeaveDate.HasValue && emp.LeaveDate.Value < today))
                {
                    status = false;
                }
                else
                {
                    status = true;
                }
                if (status != emp.IsActive)
                {
                    emp.IsActive = status;
                    await UpdateEmployee(emp);
                }
            }
        }
        public async Task<Employee?> GetEmployeeById(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var emp = await conn.QueryFirstOrDefaultAsync<Employee>(
                "SELECT * FROM Employee WHERE EmployeeId = @id", new { id });

            if (emp != null)
            {
                emp.EmployeeGrade = await GetGradeById(emp.GradeId);
                emp.EmployeeDesignation = await GetDesignationById(emp.DesignationId);
            }

            return emp;
        }
        public async Task<List<Employee>> GetAllActiveEmployeesByDesignationId(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var employees = (await conn.QueryAsync<Employee>("SELECT * FROM Employee WHERE IsActive = 1 AND DesignationId = @id", new { id })).ToList();

            foreach (var emp in employees)
            {
                emp.EmployeeGrade = await GetGradeById(emp.GradeId);
                emp.EmployeeDesignation = await GetDesignationById(emp.DesignationId);
            }

            List<Employee> groupedAndSorted = employees
                .OrderBy(e => e.EmployeeDesignation.DesignationName)
                .ThenByDescending(e => e.EmployeeGrade.GradeScore)
                .ToList();

            return groupedAndSorted;
        }
        public async Task UpdateEmployee(Employee emp)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
        UPDATE Employee SET
            EmployeeName = @EmployeeName,
            EmployeeNumber = @EmployeeNumber,
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

            await conn.ExecuteAsync("DELETE FROM Employee WHERE EmployeeId = @EmployeeId", new { emp.EmployeeId });
        }


        // ---- PRODUCT MODULE DB
        string ProductModuleTableCreationString = @"CREATE TABLE IF NOT EXISTS ProductModule(
                ModuleId INTEGER PRIMARY KEY,
                ModuleName TEXT NOT NULL,
                Rank INTEGER);";
        public async Task InsertProductModule(ProductModule module)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"INSERT INTO ProductModule (ModuleName, Rank)
                VALUES (@ModuleName, @Rank);
                SELECT last_insert_rowid();";

            var id = await conn.ExecuteScalarAsync<long>(sql, module);
            module.ModuleId = (int)id;
        }
        public async Task<List<ProductModule>> GetAllProductModules()
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM ProductModule ORDER BY Rank";
            var result = await conn.QueryAsync<ProductModule>(sql);
            return result.ToList();
        }
        public async Task<ProductModule?> GetProductModuleById(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM ProductModule WHERE ModuleId = @id";
            return await conn.QueryFirstOrDefaultAsync<ProductModule>(sql, new { id });
        }
        public async Task UpdateProductModule(ProductModule module)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"UPDATE ProductModule
                SET ModuleName = @ModuleName,
                Rank = @Rank
                WHERE ModuleId = @ModuleId";

            await conn.ExecuteAsync(sql, module);
        }
        public async Task DeleteProductModule(ProductModule module)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "DELETE FROM ProductModule WHERE ModuleId = @ModuleId";
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


        // ---- SIMULATION SCENARIO DB
        string SimulationScenarioTableCreationString = @"
                    CREATE TABLE IF NOT EXISTS SimulationScenario (
                        SimulationScenarioId INTEGER PRIMARY KEY,
                        SimulationName TEXT NOT NULL,
                        ProjectId INTEGER,
                        XMLFile TEXT NOT NULL,
                        LastThroughput REAL);";
        public async Task InsertSimulationScenario(SimulationScenario scenario)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            string sql = @"INSERT INTO SimulationScenario (SimulationName, ProjectId, XMLFile, LastThroughput)
                   VALUES (@SimulationName, @ProjectId, @XMLFile, @LastThroughput);";
            await conn.ExecuteAsync(sql, scenario);
            scenario.SimulationScenarioId = (int)conn.LastInsertRowId;
        }
        public async Task<List<SimulationScenario>> GetAllSimulationScenarios()
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            string sql = "SELECT * FROM SimulationScenario;";
            var result = await conn.QueryAsync<SimulationScenario>(sql);
            return result.ToList();
        }
        public async Task<SimulationScenario> GetSimulationScenarioById(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            string sql = "SELECT * FROM SimulationScenario WHERE SimulationScenarioId = @id;";
            return await conn.QueryFirstOrDefaultAsync<SimulationScenario>(sql, new { id });
        }
        public async Task<List<SimulationScenario>> GetSimulationScenarioByProjectId(int projectId)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            string sql = "SELECT * FROM SimulationScenario WHERE ProjectId = @projectId;";
            var result = await conn.QueryAsync<SimulationScenario>(sql, new { projectId });
            return result.ToList();
        }
        public async Task UpdateSimulationScenario(SimulationScenario scenario)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            string sql = @"UPDATE SimulationScenario
                   SET ProjectId = @ProjectId,
                        SimulationName = @SimulationName,
                        XMLFile = @XMLFile,
                        LastThroughput = @LastThroughput
                   WHERE SimulationScenarioId = @SimulationScenarioId;";
            await conn.ExecuteAsync(sql, scenario);
        }
        public async Task DeleteSimulationScenario(SimulationScenario scenario)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            string sql = "DELETE FROM SimulationScenario WHERE SimulationScenarioId = @SimulationScenarioId;";
            await conn.ExecuteAsync(sql, scenario);
        }


        // ---- DELIVERABLE DB
        public async Task InsertDeliverable(Deliverables d)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            string sql = @"INSERT INTO Deliverables (DeliverableName, DeliverableDescription, DeliverableType)
                   VALUES (@DeliverableName, @DeliverableDescription, @DeliverableType);";
            await conn.ExecuteAsync(sql, d);
            d.DeliverableId = (int)conn.LastInsertRowId;
        }
        public async Task<List<Deliverables>> GetAllDeliverables()
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            string sql = "SELECT * FROM Deliverables;";
            var result = await conn.QueryAsync<Deliverables>(sql);
            return result.ToList();
        }
        public async Task<Deliverables?> GetDeliverableById(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            string sql = "SELECT * FROM Deliverables WHERE DeliverableId = @id;";
            return await conn.QueryFirstOrDefaultAsync<Deliverables>(sql, new { id });
        }
        public async Task<Deliverables?> GetDeliverableByProject(Project project)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            string sql = "SELECT * FROM Deliverables WHERE ProjectId = @ProjectId;";
            return await conn.QueryFirstOrDefaultAsync<Deliverables>(sql, project);
        }
        public async Task UpdateDeliverable(Deliverables d)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            string sql = @"UPDATE Deliverables
                   SET DeliverableName = @DeliverableName,
                       DeliverableDescription = @DeliverableDescription,
                       DeliverableType = @DeliverableType
                   WHERE DeliverableId = @DeliverableId;";
            await conn.ExecuteAsync(sql, d);
        }
        public async Task DeleteDeliverable(Deliverables d)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            string sql = "DELETE FROM Deliverables WHERE DeliverableId = @DeliverableId;";
            await conn.ExecuteAsync(sql, d);
        }


        // ---- RESOURCE BLOCK DB
        string ResourceBlockTableCreationString = @"
                    CREATE TABLE IF NOT EXISTS ResourceBlock (
                        ResourceBlockId INTEGER PRIMARY KEY,
                        EmployeeId INTEGER NOT NULL,
                        ProjectId INTEGER,
                        Year INTEGER NOT NULL,
                        Week INTEGER NOT NULL
                    );";
        public async Task InsertResourceBlock(ResourceBlock resourceBlock)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"INSERT INTO ResourceBlock (EmployeeId, ProjectId, Year, Week)
                VALUES (@EmployeeId, @ProjectId, @Year, @Week);
                SELECT last_insert_rowid();";

            var id = await conn.ExecuteScalarAsync<long>(sql, resourceBlock);
            resourceBlock.ResourceBlockId = (int)id;
        }
        public async Task<List<ResourceBlock>> GetAllResourceBlocks()
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM ResourceBlock";
            var result = await conn.QueryAsync<ResourceBlock>(sql);
            return result.ToList();
        }
        public async Task<ResourceBlock?> GetResourceBlockById(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM ResourceBlock WHERE ResourceBlockId = @id";
            return await conn.QueryFirstOrDefaultAsync<ResourceBlock>(sql, new { id });
        }
        public async Task<ResourceBlock?> GetResourceBlockByEmployeeId(int id, int year, int week)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM ResourceBlock WHERE EmployeeId = @id AND Year = @year AND Week = @week";
            return await conn.QueryFirstOrDefaultAsync<ResourceBlock>(sql, new { id, year, week });
        }
        public async Task UpdateResourceBlock(ResourceBlock resourceBlock)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"UPDATE ResourceBlock SET 
                EmployeeId = @EmployeeId, 
                ProjectId = @ProjectId,
                Year = @Year, 
                Week = @Week 
                WHERE ResourceBlockId = @ResourceBlockId";

            await conn.ExecuteAsync(sql, resourceBlock);
        }
        public async Task DeleteResourceBlock(ResourceBlock resourceBlock)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "DELETE FROM ResourceBlock WHERE ResourceBlockId = @ResourceBlockId";
            await conn.ExecuteAsync(sql, resourceBlock);
        }

        // ---- PROJECT BLOCK DB
        string ProjectBlockTableCreationString = @"
    CREATE TABLE IF NOT EXISTS ProjectBlock(
        ProjectBlockId INTEGER PRIMARY KEY,
        ProjectId INTEGER,
        ProjectPhaseId INTEGER,
        Year INTEGER,
        Week INTEGER);";
        public async Task InsertProjectBlock(ProjectBlock block)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"INSERT INTO ProjectBlock 
        (ProjectId, ProjectPhaseId, Year, Week) 
        VALUES (@ProjectId, @ProjectPhaseId, @Year, @Week);
        SELECT last_insert_rowid();";

            var id = await conn.ExecuteScalarAsync<long>(sql, block);
            block.ProjectBlockId = (int)id;
        }
        public async Task<List<ProjectBlock>> GetAllProjectBlocks()
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM ProjectBlock";
            var result = await conn.QueryAsync<ProjectBlock>(sql);
            return result.ToList();
        }
        public async Task<ProjectBlock?> GetProjectBlockById(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM ProjectBlock WHERE ProjectBlockId = @id";
            return await conn.QueryFirstOrDefaultAsync<ProjectBlock>(sql, new { id });
        }
        public async Task<ProjectBlock?> GetProjectBlockByProjectId(int id, int year, int week)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM ProjectBlock WHERE ProjectId = @id AND Year = @year AND Week = @week";
            return await conn.QueryFirstOrDefaultAsync<ProjectBlock>(sql, new { id, year, week });
        }
        public async Task UpdateProjectBlock(ProjectBlock block)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"UPDATE ProjectBlock SET
        ProjectId = @ProjectId,
        ProjectPhaseId = @ProjectPhaseId,
        Year = @Year,
        Week = @Week
        WHERE ProjectBlockId = @ProjectBlockId";

            await conn.ExecuteAsync(sql, block);
        }
        public async Task DeleteProjectBlock(ProjectBlock block)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "DELETE FROM ProjectBlock WHERE ProjectBlockId = @ProjectBlockId";
            await conn.ExecuteAsync(sql, block);
        }



        // ---- FUNCTIONAL KPI DB
        string FunctionalKPITableCreationString = @"
                CREATE TABLE IF NOT EXISTS FunctionalKPI (
                    FunctionalKPIId INTEGER PRIMARY KEY,
                    KPIName TEXT NOT NULL,
                    KPIDescription TEXT,
                    KPIDepartment TEXT NOT NULL DEFAULT 'Engineering Design',
                    KPIEffectiveFrom INTEGER NOT NULL
                );";

        public async Task InsertFunctionalKPI(FunctionalKPI kpi)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"INSERT INTO FunctionalKPI 
    (KPIName, KPIDescription, KPIDepartment, KPIEffectiveFrom)
    VALUES (@KPIName, @KPIDescription, @KPIDepartment, @KPIEffectiveFrom);
    SELECT last_insert_rowid();";

            var id = await conn.ExecuteScalarAsync<long>(sql, kpi);
            kpi.FunctionalKPIId = (int)id;
        }
        public async Task<List<FunctionalKPI>> GetAllFunctionalKPIs()
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM FunctionalKPI";
            var result = await conn.QueryAsync<FunctionalKPI>(sql);
            return result.ToList();
        }
        public async Task<FunctionalKPI?> GetFunctionalKPIById(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM FunctionalKPI WHERE FunctionalKPIId = @id";
            return await conn.QueryFirstOrDefaultAsync<FunctionalKPI>(sql, new { id });
        }
        public async Task UpdateFunctionalKPI(FunctionalKPI kpi)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"UPDATE FunctionalKPI SET
        KPIName = @KPIName,
        KPIDescription = @KPIDescription,
        KPIDepartment = @KPIDepartment,
        KPIEffectiveFrom = @KPIEffectiveFrom
        WHERE FunctionalKPIId = @FunctionalKPIId";

            await conn.ExecuteAsync(sql, kpi);
        }
        public async Task DeleteFunctionalKPI(FunctionalKPI kpi)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "DELETE FROM FunctionalKPI WHERE FunctionalKPIId = @FunctionalKPIId";
            await conn.ExecuteAsync(sql, kpi);
        }


        // ---- PROJECT STAGE DB
        string ProjectStageTableCreationString = @"
            CREATE TABLE IF NOT EXISTS ProjectStage(
                ProjectStageId INTEGER PRIMARY KEY,
                ProjectStageName TEXT NOT NULL,
                ProjectStageDescription TEXT NOT NULL,
                Sequence INTEGER);";
        public async Task InsertProjectStage(ProjectStage stage)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"INSERT INTO ProjectStage 
    (ProjectStageName, ProjectStageDescription, Sequence)
    VALUES (@ProjectStageName, @ProjectStageDescription, @Sequence);
    SELECT last_insert_rowid();";

            var id = await conn.ExecuteScalarAsync<long>(sql, stage);
            stage.ProjectStageId = (int)id;
        }
        public async Task<List<ProjectStage>> GetAllProjectStages()
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM ProjectStage ORDER BY Sequence";
            var result = await conn.QueryAsync<ProjectStage>(sql);
            return result.ToList();
        }
        public async Task<ProjectStage?> GetProjectStageById(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM ProjectStage WHERE ProjectStageId = @id";
            return await conn.QueryFirstOrDefaultAsync<ProjectStage>(sql, new { id });
        }
        public async Task UpdateProjectStage(ProjectStage stage)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"UPDATE ProjectStage SET
        ProjectStageName = @ProjectStageName,
        ProjectStageDescription = @ProjectStageDescription,
        Sequence = @Sequence
        WHERE ProjectStageId = @ProjectStageId";

            await conn.ExecuteAsync(sql, stage);
        }
        public async Task DeleteProjectStage(ProjectStage stage)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "DELETE FROM ProjectStage WHERE ProjectStageId = @ProjectStageId";
            await conn.ExecuteAsync(sql, stage);
        }



        // ---- MILESTONE DB
        string MilestoneTableCreationString = @"
            CREATE TABLE IF NOT EXISTS Milestone(
                MilestoneId INTEGER PRIMARY KEY,
                ProjectId INTEGER,
                Name TEXT NOT NULL,
                StartDate TEXT,
                PlannedStartDate TEXT,
                RequiredDays INTEGER,
                PlannedRequiredDays INTEGER,
                DependentMilestoneId INTEGER,
                DependencyType INTEGER DEFAULT 0,
                EngineerId INTEGER,
                IsCompleted INTEGER DEFAULT 0,
                ProjectStageId INTEGER);";
        public async Task InsertMilestone(Milestone milestone)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"INSERT INTO Milestone 
        (ProjectId, Name, StartDate, PlannedStartDate, RequiredDays, PlannedRequiredDays, DependentMilestoneId, DependencyType, EngineerId, IsCompleted, ProjectStageId)
        VALUES 
        (@ProjectId, @Name, @StartDate, @PlannedStartDate, @RequiredDays, @PlannedRequiredDays, @DependentMilestoneId, @DependencyType, @EngineerId, @IsCompleted, @ProjectStageId);
        SELECT last_insert_rowid();";

            var id = await conn.ExecuteScalarAsync<long>(sql, milestone);
            milestone.MilestoneId = (int)id;
        }
        public async Task<List<Milestone>> GetAllMilestones()
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM Milestone";
            var milestones = (await conn.QueryAsync<Milestone>(sql)).ToList();

            return milestones;
        }
        public async Task<List<Milestone>> GetAllMilestonesByProjectId(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM Milestone WHERE ProjectId = @id";
            var milestones = (await conn.QueryAsync<Milestone>(sql, new { id })).ToList();

            foreach(Milestone m in milestones)
            {
                if (m.EngineerId != 0)
                {
                    m.Engineer = await GetEmployeeById(m.EngineerId);
                }
            }

            return milestones
                .OrderBy(m => m.StartDate)
                .ThenBy(m => m.StartDate.AddDays(m.RequiredDays))
                .ToList();
        }
        public async Task<List<Milestone>> GetActiveMilestonesForEngineer(int engineerId)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            string sql = @"
        SELECT * FROM Milestone
        WHERE EngineerId = @EngineerId
          AND DATE(StartDate) IS NOT NULL
          AND DATE(StartDate, '+' || RequiredDays || ' days') > DATE('now');";

            var milestones = await conn.QueryAsync<Milestone>(sql, new
            {
                EngineerId = engineerId
            });

            foreach (Milestone m in milestones)
            {
                if (m.EngineerId != 0)
                {
                    m.Project = await GetProjectById(m.ProjectId);
                    m.Engineer = await GetEmployeeById(m.EngineerId);
                    m.ProjectStage = await GetProjectStageById(m.ProjectStageId);
                }
            }

            return milestones
                .OrderBy(m => m.StartDate)
                .ThenBy(m => m.StartDate.AddDays(m.RequiredDays))
                .ToList();
        }
        public async Task FixMilestoneStartDates(int id)
        {
            List<Milestone> milestones = await GetAllMilestonesByProjectId(id);
            var milestoneDict = milestones.ToDictionary(m => m.MilestoneId);
            var dependencyGraph = milestones
                .GroupBy(m => m.DependentMilestoneId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var resolved = new HashSet<int>();
            var queue = new Queue<Milestone>();

            foreach (Milestone milestone in milestones.Where(m => m.DependentMilestoneId == 0))
            {
                AddMilestoneMissingDates(milestone);
                resolved.Add(milestone.MilestoneId);
                queue.Enqueue(milestone);
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (!dependencyGraph.ContainsKey(current.MilestoneId)) 
                    continue;

                foreach (var dependent in dependencyGraph[current.MilestoneId])
                {
                    if (resolved.Contains(dependent.MilestoneId)) continue;

                    switch (dependent.DependencyType)
                    {
                        case DependencyType.FinishToStart:
                            dependent.StartDate = CalendarLogic.AddWorkDays(current.StartDate, current.RequiredDays);
                            dependent.PlannedStartDate = CalendarLogic.AddWorkDays(current.PlannedStartDate, current.PlannedRequiredDays);
                            break;

                        case DependencyType.StartToStart:
                            dependent.StartDate = current.StartDate;
                            dependent.PlannedStartDate = current.PlannedStartDate;
                            break;

                        case DependencyType.FinishToFinish:
                            dependent.StartDate = CalendarLogic.AddWorkDays(current.StartDate, current.RequiredDays - dependent.RequiredDays);
                            dependent.PlannedStartDate = CalendarLogic.AddWorkDays(current.PlannedStartDate, current.PlannedRequiredDays - dependent.PlannedRequiredDays);
                            break;

                        case DependencyType.StartToFinish:
                            dependent.StartDate = CalendarLogic.AddWorkDays(current.StartDate, -dependent.RequiredDays);
                            dependent.PlannedStartDate = CalendarLogic.AddWorkDays(current.PlannedStartDate, -dependent.PlannedRequiredDays);
                            break;
                    }

                    AddMilestoneMissingDates(dependent);
                    _ = UpdateMilestone(dependent);

                    resolved.Add(dependent.MilestoneId);
                    queue.Enqueue(dependent);
                }
            }
        }
        public void AddMilestoneMissingDates(Milestone milestone)
        {
            milestone.EndDate = CalendarLogic.AddWorkDays(milestone.StartDate, milestone.RequiredDays);
            milestone.PlannedEndDate = CalendarLogic.AddWorkDays(milestone.PlannedStartDate, milestone.PlannedRequiredDays);
        }
        public async Task<List<Milestone>> GetMilestonesForProjectBetweenDates(int projectId,DateTime windowStart,DateTime windowEnd)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            string sql = @"
                SELECT * FROM Milestone
                WHERE ProjectId = @ProjectId
                AND DATE(StartDate) IS NOT NULL;";

            var milestones = await conn.QueryAsync<Milestone>(sql, new
            {
                ProjectId = projectId,
                StartDate = windowStart.ToString("yyyy-MM-dd"),
                EndDate = windowEnd.ToString("yyyy-MM-dd"),
            });

            List<Milestone> RelaventMilestones = [];

            foreach(Milestone milestone in milestones)
            {
                DateTime milestoneStart = milestone.StartDate;
                DateTime milestoneEnd = CalendarLogic.AddWorkDays(milestoneStart, milestone.RequiredDays);

                bool sceneA = milestoneStart >= windowStart && milestoneStart < windowEnd;
                bool sceneB = milestoneEnd >= windowStart && milestoneEnd < windowEnd;
                bool sceneC = windowStart >= milestoneStart && windowStart <= milestoneEnd;

                if (sceneA || sceneB || sceneC)
                {
                    milestone.Project = await GetProjectById(milestone.ProjectId);
                    if (milestone.EngineerId != 0)
                    {
                        milestone.Engineer = await GetEmployeeById(milestone.EngineerId);
                    }
                    if (milestone.ProjectStageId != 0)
                    {
                        milestone.ProjectStage = await GetProjectStageById(milestone.ProjectStageId);
                    }
                    RelaventMilestones.Add(milestone);
                }

            }

            return RelaventMilestones;
        }
        public async Task<List<Milestone>> GetAllMilestonesOfBlockingEngineers(int engineerId, DateTime startDate, DateTime endDate, int excludeProjectId)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            string sql = @"
        SELECT * FROM Milestone
        WHERE EngineerId = @EngineerId
          AND ProjectId != @ExcludeProjectId
          AND DATE(StartDate) IS NOT NULL;";

            var milestones = await conn.QueryAsync<Milestone>(sql, new
            {
                EngineerId = engineerId,
                ExcludeProjectId = excludeProjectId,
                StartDate = startDate.ToString("yyyy-MM-dd"),
                EndDate = endDate.ToString("yyyy-MM-dd")
            });

            List<Milestone> RelaventMilestones = [];

            foreach (Milestone milestone in milestones)
            {
                DateTime milestoneStart = milestone.StartDate;
                DateTime milestoneEnd = CalendarLogic.AddWorkDays(milestoneStart, milestone.RequiredDays);

                bool sceneA = milestoneStart >= startDate && milestoneStart < endDate;
                bool sceneB = milestoneEnd >= startDate && milestoneEnd < endDate;
                bool sceneC = startDate >= milestoneStart && startDate <= milestoneEnd;

                if (sceneA || sceneB || sceneC)
                {
                    milestone.Project = await GetProjectById(milestone.ProjectId);
                    if (milestone.EngineerId != 0)
                    {
                        milestone.Engineer = await GetEmployeeById(milestone.EngineerId);
                    }
                    if (milestone.ProjectStageId != 0)
                    {
                        milestone.ProjectStage = await GetProjectStageById(milestone.ProjectStageId);
                    }
                    RelaventMilestones.Add(milestone);
                }

            }

            return RelaventMilestones;
        }
        public async Task<Milestone?> GetMilestoneById(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM Milestone WHERE MilestoneId = @id";
            return await conn.QueryFirstOrDefaultAsync<Milestone>(sql, new { id });
        }
        public async Task UpdateMilestone(Milestone milestone)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"UPDATE Milestone SET
        ProjectId = @ProjectId,
        Name = @Name,
        StartDate = @StartDate,
        PlannedStartDate = @PlannedStartDate,
        RequiredDays = @RequiredDays,
        PlannedRequiredDays = @PlannedRequiredDays,
        DependentMilestoneId = @DependentMilestoneId,
        DependencyType = @DependencyType,
        EngineerId = @EngineerId,
        IsCompleted = @IsCompleted,
        ProjectStageId = @ProjectStageId
        WHERE MilestoneId = @MilestoneId";

            await conn.ExecuteAsync(sql, milestone);
        }
        public async Task DeleteMilestone(Milestone milestone)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "DELETE FROM Milestone WHERE MilestoneId = @MilestoneId";
            await conn.ExecuteAsync(sql, milestone);
        }


        string SpecificationTableCreationString = @"
                CREATE TABLE IF NOT EXISTS Specification(
                    SpecificationId INTEGER PRIMARY KEY,
                    SpecificationName TEXT NOT NULL,
                    SpecificationDescription TEXT,
                    ProductModuleId INTEGER,
                    ConfigurationOptions TEXT
                );";
        public async Task InsertSpecification(Specification spec)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"INSERT INTO Specification 
        (SpecificationName, SpecificationDescription, ProductModuleId, ConfigurationOptions)
        VALUES (@SpecificationName, @SpecificationDescription, @ProductModuleId, @ConfigurationOptions);
        SELECT last_insert_rowid();";

            var id = await conn.ExecuteScalarAsync<long>(sql, spec);
            spec.SpecificationId = (int)id;
        }
        public async Task<List<Specification>> GetAllSpecifications()
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"SELECT * FROM Specification";
            var result = (await conn.QueryAsync<Specification>(sql)).ToList();

            foreach (Specification specification in result)
            {
                specification.ProductModule = await GetProductModuleById(specification.ProductModuleId);
            }
            return result.OrderBy(module => module.ProductModule?.Rank).ToList();
        }
        public async Task<Specification?> GetSpecificationById(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"SELECT s.*, pm.*
                FROM Specification s
                LEFT JOIN ProductModule pm ON s.ProductModuleId = pm.ModuleId
                WHERE s.SpecificationId = @id";

            var result = await conn.QueryAsync<Specification, ProductModule, Specification>(
                sql,
                (spec, module) =>
                {
                    spec.ProductModule = module;
                    return spec;
                },
                new { id }
            );

            return result.FirstOrDefault();
        }
        public async Task<List<Specification>> GetSpecificationsByProductModuleId(int productModuleId)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"SELECT * FROM Specification WHERE ProductModuleId = @productModuleId";
            var result = await conn.QueryAsync<Specification>(sql, new { productModuleId });
            foreach (Specification specification in result)
            {
                specification.ProductModule = await GetProductModuleById(specification.ProductModuleId);
            }
            return result.ToList();
        }
        public async Task UpdateSpecification(Specification spec)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"UPDATE Specification SET
        SpecificationName = @SpecificationName,
        SpecificationDescription = @SpecificationDescription,
        ProductModuleId = @ProductModuleId,
        ConfigurationOptions = @ConfigurationOptions
        WHERE SpecificationId = @SpecificationId";

            await conn.ExecuteAsync(sql, spec);
        }
        public async Task DeleteSpecification(Specification spec)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "DELETE FROM Specification WHERE SpecificationId = @SpecificationId";
            await conn.ExecuteAsync(sql, spec);
        }


        string ConfigurationTableCreationString = @"
            CREATE TABLE IF NOT EXISTS Configuration(
                ConfigurationId INTEGER PRIMARY KEY,
                ConfigurationName TEXT NOT NULL,
                ConfigurationDescription TEXT,
                ProjectId INTEGER,
                ProductModuleId INTEGER,
                Quantity INTEGER DEFAULT 1,
                IsAddOn INTEGER DEFAULT 0,
                IsRequired INTEGER DEFAULT 1
            );";
        public async Task InsertConfiguration(Configuration config)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"INSERT INTO Configuration 
        (ConfigurationName, ConfigurationDescription, ProjectId, ProductModuleId, Quantity, IsAddOn, IsRequired)
        VALUES (@ConfigurationName, @ConfigurationDescription, @ProjectId, @ProductModuleId, @Quantity, @IsAddOn, @IsRequired);
        SELECT last_insert_rowid();";

            var id = await conn.ExecuteScalarAsync<long>(sql, config);
            config.ConfigurationId = (int)id;
        }
        public async Task<List<Configuration>> GetAllConfigurations()
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"SELECT * FROM Configuration";

            var result = await conn.QueryAsync<Configuration>(sql);

            foreach(Configuration configuration in result)
            {
                configuration.Project = await GetProjectById(configuration.ProjectId);
                configuration.ProductModule = await GetProductModuleById(configuration.ProductModuleId);
            }
            return result.ToList().OrderBy(rank => rank.ProductModule!.Rank).ToList();
        }
        public async Task<Configuration?> GetConfigurationById(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"SELECT c.*, p.*
                FROM Configuration c
                LEFT JOIN Project p ON c.ProjectId = p.ProjectId
                WHERE c.ConfigurationId = @id";

            var result = await conn.QueryAsync<Configuration, Project, Configuration>(
                sql,
                (config, project) =>
                {
                    config.Project = project;
                    return config;
                },
                new { id }
            );

            return result.FirstOrDefault();
        }
        public async Task<List<Configuration>> GetConfigurationByProjectId(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"SELECT * FROM Configuration WHERE ProjectId = @id";
            var result = await conn.QueryAsync<Configuration>(sql, new { id });

            foreach (Configuration configuration in result)
            {
                configuration.Project = await GetProjectById(configuration.ProjectId);
                configuration.ProductModule = await GetProductModuleById(configuration.ProductModuleId);
            }
            return result.ToList().OrderBy(rank => rank.ProductModule!.Rank).ToList();
        }
        public async Task UpdateConfiguration(Configuration config)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"UPDATE Configuration SET
                    ConfigurationName = @ConfigurationName,
                    ConfigurationDescription = @ConfigurationDescription,
                    ProjectId = @ProjectId,
                    ProductModuleId = @ProductModuleId,
                    Quantity = @Quantity,
                    IsAddOn = @IsAddOn,
                    IsRequired = @IsRequired
                    WHERE ConfigurationId = @ConfigurationId";

            await conn.ExecuteAsync(sql, config);
        }
        public async Task DeleteConfiguration(Configuration config)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "DELETE FROM Configuration WHERE ConfigurationId = @ConfigurationId";
            await conn.ExecuteAsync(sql, config);
        }


        string ConfigDetailTableCreationString = @"
            CREATE TABLE IF NOT EXISTS ConfigDetail(
                ConfigDetailId INTEGER PRIMARY KEY,
                ConfigurationId INTEGER,
                SpecificationId INTEGER,
                SpecificationDetail TEXT NOT NULL,
                Comments TEXT,
                Revision INTEGER,
                FirstAdded TEXT,
                LastUpdated TEXT
            );";
        public async Task InsertConfigDetail(ConfigDetail detail)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"INSERT INTO ConfigDetail 
                (ConfigurationId, SpecificationId, SpecificationDetail, Comments, Revision, FirstAdded, LastUpdated)
                VALUES (@ConfigurationId, @SpecificationId, @SpecificationDetail, @Comments, @Revision, @FirstAdded, @LastUpdated);
                SELECT last_insert_rowid();";

            var id = await conn.ExecuteScalarAsync<long>(sql, detail);
            detail.ConfigDetailId = (int)id;
        }
        public async Task<List<ConfigDetail>> GetAllConfigDetails()
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM ConfigDetail";
            var result = await conn.QueryAsync<ConfigDetail>(sql);
            foreach (ConfigDetail configDetail in result)
            {
                configDetail.Configuration = await GetConfigurationById(configDetail.ConfigDetailId);
                configDetail.Specification = await GetSpecificationById(configDetail.SpecificationId);
            }
            return result.ToList();
        }
        public async Task<List<ConfigDetail>> GetAllConfigDetailsLatestRev()
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"
        SELECT cd.*
        FROM ConfigDetail cd
        INNER JOIN (
            SELECT ConfigurationId, SpecificationId, MAX(Revision) AS MaxRev
            FROM ConfigDetail
            GROUP BY ConfigurationId, SpecificationId
        ) latest
        ON cd.ConfigurationId = latest.ConfigurationId
        AND cd.SpecificationId = latest.SpecificationId
        AND cd.Revision = latest.MaxRev;";

            var result = await conn.QueryAsync<ConfigDetail>(sql);

            foreach (ConfigDetail configDetail in result)
            {
                configDetail.Configuration = await GetConfigurationById(configDetail.ConfigurationId);
                configDetail.Specification = await GetSpecificationById(configDetail.SpecificationId);
            }
            return result.ToList();
        }
        public async Task<ConfigDetail?> GetConfigDetailById(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM ConfigDetail WHERE ConfigDetailId = @id";
            return await conn.QueryFirstOrDefaultAsync<ConfigDetail>(sql, new { id });
        }
        public async Task<ConfigDetail?> GetConfigDetailBySpecificationId(int configurationId, int specificationId)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "SELECT * FROM ConfigDetail WHERE SpecificationId = @specificationId AND ConfigurationId = @configurationId";
            return await conn.QueryFirstOrDefaultAsync<ConfigDetail>(sql, new { specificationId, configurationId });
        }
        public async Task<ConfigDetail?> GetConfigDetailBySpecificationIdLatestRev(int configurationId, int specificationId)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"
        SELECT *
        FROM ConfigDetail
        WHERE ConfigurationId = @configurationId
          AND SpecificationId = @specificationId
        ORDER BY Revision DESC
        LIMIT 1;";

            return await conn.QueryFirstOrDefaultAsync<ConfigDetail>(sql, new { specificationId, configurationId });
        }
        public async Task UpdateConfigDetail(ConfigDetail detail)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"UPDATE ConfigDetail SET
                ConfigurationId = @ConfigurationId,
                SpecificationId = @SpecificationId,
                SpecificationDetail = @SpecificationDetail,
                Comments = @Comments,
                Revision = @Revision,
                FirstAdded = @FirstAdded,
                LastUpdated = @LastUpdated
                WHERE ConfigDetailId = @ConfigDetailId";

            await conn.ExecuteAsync(sql, detail);
        }
        public async Task DeleteConfigDetail(ConfigDetail detail)
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "DELETE FROM ConfigDetail WHERE ConfigDetailId = @ConfigDetailId";
            await conn.ExecuteAsync(sql, detail);
        }













        private static string tableCreationString = @"
            

            CREATE TABLE IF NOT EXISTS Product (
                ProductId INTEGER PRIMARY KEY,
                ProductName TEXT NOT NULL);

            
            
            

            CREATE TABLE IF NOT EXISTS Grade(
                GradeId INTEGER PRIMARY KEY,
                GradeName TEXT NOT NULL,
                GradeScore INTEGER);
        
            CREATE TABLE IF NOT EXISTS Designation(
                DesignationId INTEGER PRIMARY KEY,
                DesignationName TEXT NOT NULL,
                Department TEXT);

            
            CREATE TABLE IF NOT EXISTS ReviewPoint(
                ReviewPointId INTEGER PRIMARY KEY,
                ModuleId INTEGER,
                ReviewDescription TEXT NOT NULL);

            CREATE TABLE IF NOT EXISTS ReviewItem(
                ReviewItemId INTEGER PRIMARY KEY,
                ProjectId INTEGER,
                ReviewPointId INTEGER,
                Approved INTEGER,
                LastReviewDate TEXT,
                ReviewComments TEXT,
                ReviewResponsibleID INTEGER);

            

            CREATE TABLE IF NOT EXISTS PreviousProjectCodes(
                ProjectId INTEGER,
                Code TEXT);

            CREATE TABLE IF NOT EXISTS ProjectPCodes(
                ProjectId INTEGER,
                Code TEXT);
            CREATE TABLE IF NOT EXISTS Deliverable(
                DeliverableId INTEGER,
                DeliverableName TEXT,
                DeliverableDescription TEXT,
                DeliverableType TEXT);";

    }
}
