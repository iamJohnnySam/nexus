using DataModels.DataTools;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class ResourceBlockDataAccess(string connectionString) : DataAccess<ResourceBlock>(connectionString, ResourceBlock.Metadata)
{
    public async Task<List<ResourceBlock>> GetResourceBlockByEmployeeId(int employeeId, int year, int week)
    {
        var sql = "SELECT * FROM ResourceBlock WHERE EmployeeId = @employeeId AND Year = @year AND Week = @week";
        return await QueryAsync(sql, new { employeeId, year, week });
    }

    public async Task<List<ResourceBlock>> GetResourceBlockByProjectId(int projectId, int year, int week)
    {
        var sql = "SELECT * FROM ResourceBlock WHERE ProjectId = @projectId AND Year = @year AND Week = @week";
        return await QueryAsync(sql, new { projectId, year, week });
    }

    public async Task CopyResourceBlocksFromWeek(int projectId, int copyToYear, int copyToWeek, int copyFromYear, int copyFromWeek)
    {
        var sql = "SELECT * FROM ResourceBlock WHERE ProjectId = @projectId AND Year = @copyFromYear AND Week = @copyFromWeek";
        List<ResourceBlock> items = await QueryAsync(sql, new { projectId, copyFromYear, copyFromWeek });

        foreach(ResourceBlock block in items)
        {
            block.ResourceBlockId = 0; // Reset ID for new entry
            block.Year = copyToYear;
            block.Week = copyToWeek;
            await InsertAsync(block);
        }
    }

    public async Task<List<ResourceBlock>> GetFilteredResourceBlockByProjectId(int projectId, int year, int week, string[] designationNames)
    {
        var sql = @"
            SELECT 
                rb.ResourceBlockId,
                rb.EmployeeId,
                e.EmployeeName,
                e.EmployeeNumber,
                e.GradeId,
                e.DesignationId,
                d.DesignationName,
                rb.ProjectId,
                rb.Year,
                rb.Week
            FROM ResourceBlock rb
            INNER JOIN Employee e ON rb.EmployeeId = e.EmployeeId
            INNER JOIN Designation d ON e.DesignationId = d.DesignationId
            WHERE 
                rb.ProjectId = @ProjectId
                AND rb.Year = @Year
                AND rb.Week = @Week
                AND d.DesignationName IN @DesignationNames;";

        return await QueryAsync(sql, new
        {
            ProjectId = projectId,
            Year = year,
            Week = week,
            DesignationNames = designationNames
        });
    }
}
