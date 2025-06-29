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
        private readonly string connectionString;

        public SQLiteConnection Connection
        {
            get { return new SQLiteConnection(connectionString); }
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
                using var connection = new SQLiteConnection(connectionString);
                connection.Open();
                return GetAllProjects().Result;
            }
        }


        public Manager()
        {
            connectionString = $"Data Source={dbPath};Version=3;";

            bool databaseNotExist = false;
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
                databaseNotExist = true;
            }

            if (databaseNotExist)
            {
                using var connection = new SQLiteConnection(connectionString);
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

        public async Task InsertCustomer(Customer p)
        {
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();

            string sql = @"INSERT INTO Customer (CustomerName)
                       VALUES (@CustomerName);";
            await conn.ExecuteAsync(sql, p);
            p.CustomerId = (int)conn.LastInsertRowId;
        }
        public async Task<List<Customer>> GetAllCustomers()
        {
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();
            return (await conn.QueryAsync<Customer>("SELECT * FROM Customer")).ToList();
        }

        public async Task InsertProduct(Product p)
        {
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();

            string sql = @"INSERT INTO Product (ProductName)
                       VALUES (@ProductName);";
            await conn.ExecuteAsync(sql, p);
            p.ProductId = (int)conn.LastInsertRowId;
        }
        public async Task<List<Product>> GetAllProducts()
        {
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();
            return (await conn.QueryAsync<Product>("SELECT * FROM Product")).ToList();
        }

        public Project GetNewProject()
        {
            return new Project { ProjectName = "Untitled Project" };
        }
        public async Task InsertProject(Project p)
        {
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();

            string sql = @"INSERT INTO Project (ProjectName, CustomerId, DesignCode, Priority, POStatus, ProductId)
                       VALUES (@ProjectName, @CustomerId, @DesignCode, @Priority, @POStatus, @ProductId);";
            await conn.ExecuteAsync(sql, p);
            p.ProjectId = (int)conn.LastInsertRowId;
        }
        public async Task<Project?> GetProject(int projectId)
        {
            using var conn = new SQLiteConnection(connectionString);
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
            using var conn = new SQLiteConnection(connectionString);
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
        public void UpdateProject(Project p)
        {
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();

            string sql = @"UPDATE Project
                       SET ProjectName = @ProjectName,
                           CustomerID = @CustomerID,
                           DesignCode = @DesignCode,
                           Priority = @Priority,
                           POStatus = @POStatus,
                           ProductId = @ProductId
                       WHERE ProjectId = @ProjectId;";
            conn.Execute(sql, p);
        }
        public void DeleteProject(int projectId)
        {
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();

            string sql = "DELETE FROM Project WHERE ProjectId = @ProjectId;";
            conn.Execute(sql, new { ProjectId = projectId });
        }

        public async Task InsertTask(TaskItem p)
        {
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();

            if(p.ProjectId == 0)
                p.ProjectId = currentProject.ProjectId;

            string sql = @"INSERT INTO TaskItem (ProjectId, Title, Responsible, CreatedOn, Deadline, IsCompleted, ParentTaskId)
                       VALUES (@ProjectId, @Title, @Responsible, @CreatedOn, @Deadline, @IsCompleted, @ParentTaskId);";
            await conn.ExecuteAsync(sql, p);
            p.TaskId = (int)conn.LastInsertRowId;
        }
        public async Task<List<TaskItem>> GetAllTasks_L1()
        {
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();
            return (await conn.QueryAsync<TaskItem>("SELECT * FROM TaskItem")).ToList();
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
                ProductId INTEGER,
                FOREIGN KEY (CustomerId) REFERENCES Customer(CustomerId)
                FOREIGN KEY (ProductId) REFERENCES Product(ProductId)
            );
            
            CREATE TABLE IF NOT EXISTS Module(
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
                ReplacedEmployeeId INTEGER,
                FOREIGN KEY (GradeId) REFERENCES Grade(GradeId),
                FOREIGN KEY (DesignationId) REFERENCES Designation(DesignationId),
                FOREIGN KEY (ReplacedEmployeeId) REFERENCES Employee(EmployeeId)
            );

            CREATE TABLE IF NOT EXISTS ReviewPoint(
                ReviewPointId INTEGER PRIMARY KEY,
                ModuleId INTEGER,
                ReviewDescription TEXT NOT NULL,
                FOREIGN KEY (ModuleId) REFERENCES Module(ModuleId)
            );

            CREATE TABLE IF NOT EXISTS ReviewItem(
                ReviewItemId INTEGER PRIMARY KEY,
                ProjectId INTEGER,
                ReviewPointId INTEGER,
                Approved INTEGER,
                LastReviewDate TEXT,
                ReviewComments TEXT,
                ReviewResponsibleID INTEGER,
                FOREIGN KEY (ProjectId) REFERENCES Project(ProjectId),
                FOREIGN KEY (ReviewPointId) REFERENCES ReviewPoint(ReviewPointId),
                FOREIGN KEY (ReviewResponsibleID) REFERENCES Employee(EmployeeId)
            );

            CREATE TABLE IF NOT EXISTS TaskItem(
                TaskId INTEGER PRIMARY KEY,
                ProjectId INTEGER,
                Title TEXT NOT NULL,
                CreatedOn TEXT,
                Deadline TEXT NOT NULL,
                IsCompleted INTEGER,
                ParentTaskId INTEGER,
                FOREIGN KEY (ProjectId) REFERENCES Project(ProjectId),
                FOREIGN KEY (ParentTaskId) REFERENCES TaskItem(TaskId)
            );

            CREATE TABLE IF NOT EXISTS TaskItemResponsibility(
                TaskId INTEGER,
                EmployeeId INTEGER,
                FOREIGN KEY (TaskId) REFERENCES TaskItem(TaskId),
                FOREIGN KEY (EmployeeId) REFERENCES Employee(EmployeeId)
            );

            CREATE TABLE IF NOT EXISTS PreviousProjectCodes(
                ProjectId INTEGER,
                Code TEXT,
                FOREIGN KEY (ProjectId) REFERENCES Project(ProjectId)
            );

            CREATE TABLE IF NOT EXISTS ProjectPCodes(
                ProjectId INTEGER,
                Code TEXT,
                FOREIGN KEY (ProjectId) REFERENCES Project(ProjectId)
            );";

    }
}
