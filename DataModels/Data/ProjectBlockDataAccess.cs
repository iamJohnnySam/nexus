using DataModels.DataTools;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class ProjectBlockDataAccess(string connectionString) : DataAccess<ProjectBlock>(connectionString, ProjectBlock.Metadata)
{
    public async Task<ProjectBlock?> GetProjectBlockByProjectId(int id, int year, int week)
    {
        var sql = "SELECT * FROM ProjectBlock WHERE ProjectId = @id AND Year = @year AND Week = @week";
        return await QueryFirstOrDefaultAsync(sql, new { id, year, week });
    }
}
