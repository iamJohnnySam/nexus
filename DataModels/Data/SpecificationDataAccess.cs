using DataModels.DataTools;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class SpecificationDataAccess(string connectionString, ProductModuleDataAccess productModuleDB) : DataAccess<Specification>(connectionString, Specification.Metadata)
{
    ProductModuleDataAccess ProductModuleDB = productModuleDB;

    public override async Task<List<Specification>> GetAllAsync(string? orderBy = null, bool descending = false)
    {
        var items = await base.GetAllAsync(orderBy, descending);
        foreach (Specification item in items)
        {
            item.ProductModule = await ProductModuleDB.GetByIdAsync(item.ProductModuleId);
        }
        return items.OrderBy(rank => rank.ProductModule!.Rank).ToList();
    }

    public override async Task<Specification?> GetByIdAsync(object id)
    {
        var item = await base.GetByIdAsync(id);
        if (item != null)
            item.ProductModule = await ProductModuleDB.GetByIdAsync(item.ProductModuleId);
        return item;
    }

    public async Task<List<Specification>> GetByProductModuleIdAsync(int productModuleId)
    {
        var items = await GetByColumnAsync(nameof(Specification.ProductModuleId), productModuleId);
        foreach (Specification item in items)
        {
            item.ProductModule = await ProductModuleDB.GetByIdAsync(item.ProductModuleId);
        }
        return items.OrderBy(rank => rank.ProductModule!.Rank).ToList();
    }
}
