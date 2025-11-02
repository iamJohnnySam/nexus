using DataModels.DataTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class GradeDataAccess(string connectionString) : DataAccess<Grade>(connectionString, Grade.Metadata)
{
    public Dictionary<int, Grade> GradeCache { get; set; } = [];

    public override async Task<Grade?> GetByIdAsync(int id)
    {
        if (!GradeCache.ContainsKey(id))
        {
            GradeCache[id] = (await base.GetByIdAsync(id))!;
        }
        return GradeCache[id];
    }

    public override async Task UpdateAsync (Grade entity)
    {
        await base.UpdateAsync(entity);
        GradeCache[entity.GradeId] = entity;
    }

    public override async Task DeleteAsync (Grade entity)
    {
        await base.DeleteAsync(entity);
        GradeCache.Remove(entity.GradeId);
    }
}
