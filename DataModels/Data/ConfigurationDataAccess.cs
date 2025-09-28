using DataModels.DataTools;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
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
    public async Task<List<Configuration>> GetByProjectId(int projectId)
    {
        var items = await GetByColumnAsync(nameof(Configuration.ProjectId), projectId);
        foreach(Configuration item in  items)
        {
            await GetItems(item);
        }
        return items;
    }

    public override async Task<List<Configuration>> GetAllAsync(string? orderBy = null, bool descending = false)
    {
        var items = await base.GetAllAsync(orderBy, descending);
        foreach (Configuration item in items)
        {
            await GetItems(item);
        }
        return items.OrderBy(rank => rank.ProductModule!.Rank).ToList();
    }

    public override async Task<Configuration?> GetByIdAsync(object id)
    {
        var item = await base.GetByIdAsync(id);
        if (item != null)
            await GetItems(item);
        return item;
    }
}
