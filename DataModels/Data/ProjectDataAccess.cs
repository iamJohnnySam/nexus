using DataModels.Administration;
using DataModels.DataTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class ProjectDataAccess(string connectionString, CustomerDataAccess customerDB, ProductDataAccess productDB, EmployeeDataAccess employeeDB) : DataAccess<Project>(connectionString, Project.Metadata)
{
    private CustomerDataAccess CustomerDB { get; } = customerDB;
    private ProductDataAccess ProductDB { get; } = productDB;
    private EmployeeDataAccess EmployeeDB { get; } = employeeDB;

    public static Project GetNew()
    {
        return new Project { ProjectName = "Untitled Project" };
    }
    public async Task GetProjectObjects(Project project)
    {
        project.Customer = await CustomerDB.GetByIdAsync(project.CustomerId);
        project.Product = await ProductDB.GetByIdAsync(project.ProductId);
        project.PrimaryDesigner = await EmployeeDB.GetByIdAsync(project.PrimaryDesignerId);
    }
    public async override Task<Project?> GetByIdAsync(object projectId)
    {
        var project = await base.GetByIdAsync(projectId);

        if (project != null)
            await GetProjectObjects(project);
        else
            throw new Exception($"Project with ID '{projectId}' not found.");

        return project;
    }
    public async override Task<List<Project>> GetAllAsync(string? orderBy = null, bool descending = false)
    {
        var projects = await base.GetAllAsync(orderBy, descending);

        foreach (var project in projects)
        {
            await GetProjectObjects(project);
        }

        return projects;
    }
    public async Task<List<Project>> GetAllActiveAsync()
    {
        List<Project> projects = await GetByColumnAsync("IsActive", true, "ProjectName", true);

        foreach (var project in projects)
        {
            await GetProjectObjects(project);
        }

        return projects;
    }
    public async Task<List<Project>> GetAllTrackedAsync()
    {
        List<Project> projects = await GetByColumnAsync("IsTrackedProject", true, "ProjectName", true);

        foreach (var project in projects)
        {
            await GetProjectObjects(project);
        }

        return projects;
    }
    public async Task<List<Project>> GetAllActiveTrackedAsync()
    {

        var sql = @"SELECT * FROM Project 
                        WHERE IsActive = 1 AND IsTrackedProject = 1
                        ORDER BY ProjectName ASC;";
        List<Project> projects = await QueryAsync(sql);

        foreach (var project in projects)
        {
            await GetProjectObjects(project);
        }

        return projects;
    }
    public async Task<List<string>> GetAllProjectNamesAsync()
    {
        return [.. (await GetAllAsync()).Select(item => item?.GetType().GetProperty("ProjectName")?.GetValue(item)?.ToString()).Where(projectName => projectName != null)];
    }
    public async Task<List<string>> GetAllActiveProjectNamesAsync()
    {
        return CompileProjectNames(await (GetAllActiveAsync()));
    }
    public async Task<List<string>> GetAllActiveTrackedProjectNamesAsync()
    {
        return CompileProjectNames(await (GetAllActiveTrackedAsync()));
    }
    private static List<string> CompileProjectNames(List<Project> projects)
    {
        List<string> projectNames = [];
        foreach (Project project in projects)
        {
            projectNames.Add(CompileProjectName(project));
        }
        return projectNames;
    }
    public static string CompileProjectName(Project project)
    {
        string name = string.Empty;
        if (project.DesignCode != null && project.DesignCode != string.Empty)
            name = $"{project.DesignCode} | ";
        name += project.ProjectName;
        return name;
    }
    public async Task<Project> SelectProjectFromName(string projectName, LoginInformation LoginInfo)
    {
        Project project = await GetOneByColumnAsync("ProjectName", projectName) ?? throw new Exception($"Project with name '{projectName}' not found.");

        if (project != null)
        {
            await GetProjectObjects(project);
            LoginInfo.CurrentProject = project;
        }
        else
        {
            throw new Exception($"Project with name '{projectName}' not found.");
        }
        return project;
    }
}