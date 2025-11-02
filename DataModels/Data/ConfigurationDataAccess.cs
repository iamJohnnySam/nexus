using DataModels.DataTools;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class ConfigurationDataAccess(string connectionString, ProjectDataAccess projectDB, ProductModuleDataAccess productModuleDB) : DataAccess<Configuration>(connectionString, Configuration.Metadata)
{
    ProjectDataAccess ProjectDB = projectDB;
    ProductModuleDataAccess ProductModuleDB = productModuleDB;
    private async Task GetItems(Configuration config)
    {
        config.Project = await ProjectDB.GetByIdAsync(config.ProjectId);
        config.ProductModule = await ProductModuleDB.GetByIdAsync(config.ProductModuleId);
    }
    public async Task<List<Configuration>> GetByProjectIdAsync(int projectId, bool getObjects = true)
    {
        var items = await GetByColumnAsync(nameof(Configuration.ProjectId), projectId);
        if (getObjects)
        {
            foreach (Configuration item in items)
            {
                await GetItems(item);
            }
        }
        return items.OrderBy(rank => rank.ProductModule!.Rank).ToList();
    }

    internal override async Task GetAllAsync()
    {
        await base.GetAllAsync();
        foreach (Configuration item in AllItems)
        {
            await GetItems(item);
        }
    }

    public override async Task<Configuration?> GetByIdAsync(int id)
    {
        var item = await base.GetByIdAsync(id);
        if (item != null)
            await GetItems(item);
        return item;
    }

    public async Task<List<ProductModule>> GetProductModulesByProjectIdAsync(int ProjectId)
    {
        var items = await GetByProjectIdAsync(ProjectId, false);
        List<int> moduleIds = [];
        List<ProductModule> modules = [];
        foreach (var item in items)
        {
            if (!moduleIds.Contains(item.ProductModuleId))
            {
                moduleIds.Add(item.ProductModuleId);
                modules.Add((await ProductModuleDB.GetByIdAsync(item.ProductModuleId))!);
            }
        }
        return modules.OrderBy(module => module.Rank).ToList();
    }
}
