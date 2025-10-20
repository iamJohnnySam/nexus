using DataModels.DataTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class MilestoneDataAccess(string connectionString) : DataAccess<Milestone>(connectionString, Milestone.Metadata)
{
    public async Task<List<Milestone>> GetByProjectIdAsync(int projectId)
    {
        return await GetByColumnAsync("ProjectId", projectId);
    }
    public async Task<List<Milestone>> GetOpenByProjectIdAsync(int projectId)
    {
        string sql = @"SELECT * FROM Milestone 
                        WHERE IsCompleted = 0 AND ProjectId = @ProjectId;";
        return await QueryAsync(sql, new {ProjectId = projectId});
    }
    public async Task<List<Milestone>> GetCompletedByProjectIdAsync(int projectId)
    {
        string sql = @"SELECT * FROM Milestone 
                        WHERE IsCompleted = 1 AND ProjectId = @ProjectId;";
        return await QueryAsync(sql, new { ProjectId = projectId });
    }
}
