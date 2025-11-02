using DataModels.DataTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class DesignationDataAccess : DataAccess<Designation>
{
    private Dictionary<int, Designation> DesignationCache { get; set; } = [];
    public DesignationDataAccess(string connectionString) : base(connectionString, Designation.Metadata)
    {
    }

    public override async Task<Designation?> GetByIdAsync(int id)
    {
        if (!DesignationCache.ContainsKey(id))
        {
            DesignationCache[id] = (await base.GetByIdAsync(id))!;
        }
        return DesignationCache[id];
    }

    public override async Task UpdateAsync (Designation entity)
    {
        await base.UpdateAsync(entity);
        DesignationCache[entity.DesignationId] = entity;
    }

    public override async Task DeleteAsync (Designation entity)
    {
        await base.DeleteAsync(entity);
        DesignationCache.Remove(entity.DesignationId);
    }
}
