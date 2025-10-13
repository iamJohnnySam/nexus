using DataModels.DataTools;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class ReviewPointDataAccess(string connectionString) : DataAccess<ReviewPoint>(connectionString, ReviewPoint.Metadata)
{
    ProductModuleDataAccess ProductModuleDB = new ProductModuleDataAccess(connectionString);
    public override async Task<List<ReviewPoint>> GetAllAsync (string? orderBy = null, bool descending = false)
    {
        List<ReviewPoint> points = await base.GetAllAsync(orderBy, descending);
        foreach (ReviewPoint point in points)
        {
            point.Module = await ProductModuleDB.GetByIdAsync(point.ModuleId);
        }
        return points.OrderBy(rank => rank.Module!.Rank).ThenBy(cat => cat.ReviewCategory).ToList();
    }
    public async Task<List<ReviewPoint>> GetByProductModuleIdAsync(int ProductModuleId)
    {
        return (await GetByColumnAsync(nameof(ReviewPoint.ModuleId), ProductModuleId)).GroupBy(cat => cat.ReviewCategory).SelectMany(group => group).ToList();
    }
}
   