using DataModels.Administration;
using DataModels.Data;
using DataModels.Tools;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Runtime.InteropServices;

namespace DataModels;

public class Manager
{
    public LoginInstance LoginInfo { get; }

    private readonly string dbPath = "NexusDB.sqlite";
    private readonly string _connectionString;

    // DataAccess
    public ConfigDetailDataAccess ConfigDetailDB { get; }
    public ConfigurationDataAccess ConfigurationDB { get; }
    public CustomerDataAccess CustomerDB { get; }
    public DeliverableDataAccess DeliverableDB { get; }
    public DesignationDataAccess DesignationDB { get; }
    public EmployeeDataAccess EmployeeDB { get; }
    public FunctionalKpiDataAccess FunctionalKpiDB { get; }
    public GradeDataAccess GradeDB { get; }
    public LoginDataAccess LoginDB { get; }
    public MilestoneDataAccess MilestoneDB { get; }
    public OEMItemDataAccess OEMItemDB { get; }
    public ProductDataAccess ProductDB { get; }
    public ProductModuleDataAccess ProductModuleDB { get; }
    public ProjectDataAccess ProjectDB { get; }
    public ProjectBlockDataAccess ProjectBlockDB { get; }
    public ProjectStageDataAccess ProjectStageDB { get; }
    public ResourceBlockDataAccess ResourceBlockDB { get; }
    public ReviewItemDataAccess ReviewItemDB { get; }
    public ReviewPointDataAccess ReviewPointDB { get; }
    public SimulationScenarioDataAccess SimulationScenarioDB { get; }
    public SimulationStationDataAccess SimulationStationDB { get; }
    public SimulationManipulatorDataAccess SimulationManipulatorDB { get; }
    public SimulationProcessDataAccess SimulationProcessDB { get; }
    public SpecificationDataAccess SpecificationDB { get; }
    public SupplierDataAccess SupplierDB { get; }
    public TaskItemDataAccess TaskItemDB { get; }
    public FlowElementDataAccess FlowElementDB { get; }

    public Manager()
    {
        string location;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            location = Path.Combine(homeDir, dbPath);
        }
        else
        {
            location = dbPath;
        }
        _connectionString = $"Data Source={location};";
        

        bool dbJustCreated = false;
        if (!string.IsNullOrEmpty(location) && !File.Exists(dbPath))
        {
            dbJustCreated = true;
        }


        // Employee
        DesignationDB = new(_connectionString);
        GradeDB = new(_connectionString);
        EmployeeDB = new(_connectionString, GradeDB, DesignationDB);
        LoginDB = new(_connectionString);

        CustomerDB = new(_connectionString);
        ProductDB = new(_connectionString);

        ProjectDB = new(_connectionString, CustomerDB, ProductDB, EmployeeDB);
        ProjectDB.CurrentProjectChanged += ProjectDB_CurrentProjectChanged;


        if (dbJustCreated)
        {
            InitializeDatabase();
        }

        LoginInfo = new() { 
            CurrentProject = ProjectDB.GetByIdAsync(1).Result! 
        };

        ProductModuleDB = new(_connectionString);
        ConfigurationDB = new(_connectionString, ProjectDB, ProductModuleDB);
        SpecificationDB = new(_connectionString, ProductModuleDB);
        ConfigDetailDB = new(_connectionString, ConfigurationDB, SpecificationDB);
        

        // Tasks
        TaskItemDB = new(_connectionString, LoginInfo, EmployeeDB);

        ProjectStageDB = new(_connectionString);

        DeliverableDB = new(_connectionString);
        
        
        FunctionalKpiDB = new(_connectionString);
        
        
        
        MilestoneDB = new(_connectionString, EmployeeDB, ProjectDB, ProjectStageDB);
        OEMItemDB = new(_connectionString);
        
        
        ProjectBlockDB = new(_connectionString);
        
        ResourceBlockDB = new(_connectionString);
        ReviewItemDB = new(_connectionString);
        ReviewPointDB = new(_connectionString);

        SimulationScenarioDB = new(_connectionString);
        SimulationStationDB = new(_connectionString);
        SimulationManipulatorDB = new(_connectionString);
        SimulationProcessDB = new(_connectionString);

        SupplierDB = new(_connectionString);
        FlowElementDB = new(_connectionString);
    }

    private void InitializeDatabase()
    {
        Customer newCustomer = new() { CustomerName = "Internal" };
        CustomerDB.InsertAsync(newCustomer).Wait();

        Product newProduct = new() { ProductName = "None" };
        ProductDB.InsertAsync(newProduct).Wait();

        Project newProject = new()
        {
            ProjectName = "General",
            CustomerId = newCustomer.CustomerId,
            DesignCode = "GENERAL",
            Priority = EProjectPriority.Normal,
            POStatus = ESalesStatus.Concept,
            ProductId = newProduct.ProductId,
        };
        ProjectDB.InsertAsync(newProject).Wait();
    }

    private void ProjectDB_CurrentProjectChanged(object? sender, Project e)
    {
        LoginInfo.CurrentProject = e;
    }
}
