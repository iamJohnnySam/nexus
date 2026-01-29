using DataModels.Data;
using DataModels.Tools;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using NexusMaintenance;
using System.IO;
using System.Runtime.InteropServices;

namespace DataModels;

public class Manager
{
    private readonly string dbFileName = "NexusDB.sqlite";
    public readonly string dbPath;
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
    public TimelineItemDataAccess TimelineItemDB { get; }
    public MilestoneDataAccess MilestoneDB { get; }
    public MilestoneTemplateDataAccess MilestoneTemplateDB { get; }
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
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            dbPath = Path.Combine(homeDir, dbFileName);
        }
        else
        {
            dbPath = dbFileName;
        }
        _connectionString = $"Data Source={dbPath};";

        // Employee
        DesignationDB = new(_connectionString);
        GradeDB = new(_connectionString);
        EmployeeDB = new(_connectionString);
        LoginDB = new(_connectionString);

        CustomerDB = new(_connectionString);
        ProductDB = new(_connectionString);

        ProjectDB = new(_connectionString);


        if (ProjectDB.GetByIdAsync(1).Result == null)
        {
            InitializeDatabase();
        }

        ProductModuleDB = new(_connectionString);
        ConfigurationDB = new(_connectionString, ProjectDB, ProductModuleDB);
        SpecificationDB = new(_connectionString, ProductModuleDB);
        ConfigDetailDB = new(_connectionString, ConfigurationDB, SpecificationDB);
        

        // Tasks
        TaskItemDB = new(_connectionString);

        ProjectStageDB = new(_connectionString);

        DeliverableDB = new(_connectionString);
        
        
        FunctionalKpiDB = new(_connectionString);



        TimelineItemDB = new(_connectionString, EmployeeDB, ProjectDB, ProjectStageDB);
        MilestoneDB = new(_connectionString);
        MilestoneTemplateDB = new(_connectionString);
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

        new SqliteLogger().Info($"Manager Created");
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
}
