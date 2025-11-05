using DataModels.DataTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class ProjectDataAccess(string connectionString) : DataAccess<Project>(connectionString, Project.Metadata)
{
    private CustomerDataAccess CustomerDB { get; } = new(connectionString);
    private ProductDataAccess ProductDB { get; } = new(connectionString);
    private EmployeeDataAccess EmployeeDB { get; } = new(connectionString);

    // Cached Data
    private List<Project> activeTracked = [];
    public List<Project> ActiveTracked
    {
        get
        {
            if(activeTracked.Count == 0)
            {
                Task.Run(async () => await GetAllActiveTrackedAsync()).Wait();
            }
            return activeTracked;
        }
        set
        {
            activeTracked = value;
            OnPropertyChanged();
        }
    }

    public static Project GetNew()
    {
        return new Project { ProjectName = "Untitled Project" };
    }
    
    internal override async Task ReloadCachedData()
    {
        await base.ReloadCachedData();
        await GetAllActiveTrackedAsync();
    }

    public async Task GetProjectObjects(Project project)
    {
        project.Customer = await CustomerDB.GetByIdAsync(project.CustomerId);
        project.Product = await ProductDB.GetByIdAsync(project.ProductId);
        project.PrimaryDesigner = await EmployeeDB.GetByIdAsync(project.PrimaryDesignerId);
    }
    public async override Task<Project?> GetByIdAsync(int projectId)
    {
        var project = await base.GetByIdAsync(projectId);

        if (project != null)
            await GetProjectObjects(project);
        else
            throw new Exception($"Project with ID '{projectId}' not found.");

        return project;
    }
    internal async override Task GetAllAsync()
    {
        await base.GetAllAsync();

        foreach (var project in AllItems)
        {
            await GetProjectObjects(project);
        }

        AllItems = AllItems.OrderByDescending(p => p.IsTrackedProject)
                           .ThenByDescending(p => p.IsActive)
                           .ThenByDescending(p => p.ProjectCode)
                           .ThenBy(p => p.DesignCode)
                           .ToList();
    }
    public async Task<List<Project>> GetAllActiveAsync()
    {
        List<Project> projects = await GetByColumnAsync("IsActive", true);

        foreach (var project in projects)
        {
            await GetProjectObjects(project);
        }

        return projects.OrderByDescending(p => p.IsTrackedProject)
                           .ThenByDescending(p => p.IsActive)
                           .ThenByDescending(p => p.ProjectCode)
                           .ThenBy(p => p.DesignCode)
                           .ToList();
    }
    public async Task<List<Project>> GetAllTrackedAsync()
    {
        List<Project> projects = await GetByColumnAsync("IsTrackedProject", true);

        foreach (var project in projects)
        {
            await GetProjectObjects(project);
        }

        return projects.OrderByDescending(p => p.IsTrackedProject)
                           .ThenByDescending(p => p.IsActive)
                           .ThenByDescending(p => p.ProjectCode)
                           .ThenBy(p => p.DesignCode)
                           .ToList();
    }
    public async Task GetAllActiveTrackedAsync()
    {

        var sql = @"SELECT * FROM Project WHERE IsActive = 1 AND IsTrackedProject = 1 ORDER BY ProjectName ASC;";
        List<Project> projects = await QueryAsync(sql);

        foreach (var project in projects)
        {
            await GetProjectObjects(project);
        }

        ActiveTracked = projects.OrderByDescending(p => p.IsTrackedProject)
                           .ThenByDescending(p => p.IsActive)
                           .ThenByDescending(p => p.ProjectCode)
                           .ThenBy(p => p.DesignCode)
                           .ToList();
    }
    public List<string> GetAllProjectNamesAsync()
    {
        return [.. AllItems.Select(item => item?.GetType().GetProperty("ProjectName")?.GetValue(item)?.ToString()).Where(projectName => projectName != null)];
    }
    public async Task<List<string>> GetAllActiveProjectNamesAsync()
    {
        return CompileProjectNames(await (GetAllActiveAsync()));
    }
    public List<string> GetAllActiveTrackedProjectNames()
    {
        return CompileProjectNames(ActiveTracked);
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
    public async Task<Project> SelectProjectFromName(string projectName)
    {
        Project project = await GetOneByColumnAsync("ProjectName", projectName) ?? throw new Exception($"Project with name '{projectName}' not found.");

        if (project != null)
        {
            await GetProjectObjects(project);
        }
        else
        {
            throw new Exception($"Project with name '{projectName}' not found.");
        }
        return project;
    }
}