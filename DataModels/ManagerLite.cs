using DataModels.Data;
using NexusMaintenance;
using System.Runtime.InteropServices;

namespace DataModels;

public class ManagerLite
{
    private readonly string dbFileName = "NexusDB.sqlite";
    public readonly string dbPath;
    private readonly string _connectionString;

    // DataAccess
    public ProductDataAccess ProductDB { get; }
    public CustomerDataAccess CustomerDB { get; }
    public DesignationDataAccess DesignationDB { get; }
    public EmployeeDataAccess EmployeeDB { get; }
    public GradeDataAccess GradeDB { get; }
    public ProjectDataAccess ProjectDB { get; }

    public ManagerLite()
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
        EmployeeDB = new(_connectionString, GradeDB, DesignationDB);
        CustomerDB = new(_connectionString);
        ProductDB = new(_connectionString);
        ProjectDB = new(_connectionString, CustomerDB, ProductDB, EmployeeDB);
        new SqliteLogger().InfoAsync($"Manager Lite Created");
    }
}
