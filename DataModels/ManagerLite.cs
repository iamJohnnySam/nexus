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
    public EmployeeDataAccess EmployeeDB { get; }
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
        EmployeeDB = new(_connectionString);
        ProjectDB = new(_connectionString);
        new SqliteLogger().InfoAsync($"Manager Lite Created");
    }
}
