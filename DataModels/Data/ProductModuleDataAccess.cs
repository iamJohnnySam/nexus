using DataModels.DataTools;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class ProductModuleDataAccess(string connectionString) : DataAccess<ProductModule>(connectionString, ProductModule.Metadata)
{
    public override async Task<List<ProductModule>> GetAllAsync(string? orderBy = null, bool descending = false)
    {
        List < ProductModule > temp = await base.GetAllAsync(orderBy, descending);
        return temp.OrderBy(mod => mod.Rank).ToList();
    }
}
