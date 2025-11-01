﻿using DataModels.DataTools;
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
    internal override async Task GetAllAsync()
    {
        await base.GetAllAsync();
        foreach (ReviewPoint point in AllItems)
        {
            point.Module = await ProductModuleDB.GetByIdAsync(point.ModuleId);
        }
        AllItems = AllItems.OrderBy(rank => rank.Module!.Rank).ThenBy(cat => cat.ReviewCategory).ToList();
    }
    public async Task<List<ReviewPoint>> GetByProductModuleIdAsync(int ProductModuleId)
    {
        return (await GetByColumnAsync(nameof(ReviewPoint.ModuleId), ProductModuleId)).GroupBy(cat => cat.ReviewCategory).SelectMany(group => group).ToList();
    }
}
   