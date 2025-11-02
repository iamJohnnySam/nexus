using DataModels.DataTools;
using DataModels.Tools;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class ResourceBlockDataAccess(string connectionString) : DataAccess<ResourceBlock>(connectionString, ResourceBlock.Metadata)
{
    public Dictionary<int, ResourceBlockLink> EmployeeResourceBlocks { get; set; } = [];
    public Dictionary<int, ResourceBlockLink> ProjectResourceBlocks { get; set; } = [];

    private int GetEmployeeLinkKey(int employeeId, int year, int week) => (employeeId * 10000) + (year * 100) + week;
    private int GetProjectLinkKey(int projectId, int year, int week) => (projectId * 10000) + (year * 100) + week;

    public async Task<ResourceBlockLink> GetEmployeeResourceBlockLink(int employeeId, int year, int week)
    {
        (year , week) = CalendarLogic.WeekValidation(year, week);
        int key = GetEmployeeLinkKey(employeeId, year, week);
        if (!EmployeeResourceBlocks.ContainsKey(key))
        {
            EmployeeResourceBlocks[key] = new ResourceBlockLink(await GetResourceBlockByEmployeeId(employeeId, year, week));
        }
        return EmployeeResourceBlocks[key];
    }

    public async Task<ResourceBlockLink> GetProjectResourceBlockLink(int projectId, int year, int week)
    {
        (year, week) = CalendarLogic.WeekValidation(year, week);
        int key = GetProjectLinkKey(projectId, year, week);
        if (!ProjectResourceBlocks.ContainsKey(key))
        {
            ProjectResourceBlocks[key] = new ResourceBlockLink(await GetResourceBlockByProjectId(projectId, year, week));
        }
        return ProjectResourceBlocks[key];
    }

    private async Task UpdateCache(ResourceBlock item)
    {
        int EmployeeKey = GetEmployeeLinkKey(item.EmployeeId, item.Year, item.Week);
        if (EmployeeResourceBlocks.ContainsKey(EmployeeKey))
            EmployeeResourceBlocks[EmployeeKey].Blocks = await GetResourceBlockByEmployeeId(item.EmployeeId, item.Year, item.Week);
        int ProjectKey = GetProjectLinkKey(item.ProjectId, item.Year, item.Week);
        if (ProjectResourceBlocks.ContainsKey(ProjectKey))
            ProjectResourceBlocks[ProjectKey].Blocks = await GetResourceBlockByProjectId(item.ProjectId, item.Year, item.Week);
    }

    public override async Task InsertAsync(ResourceBlock item)
    {
        await base.InsertAsync(item);
        await UpdateCache(item);
    }

    public override async Task UpdateAsync(ResourceBlock item)
    {
        await base.UpdateAsync(item);
        await UpdateCache(item);
    }

    public override async Task DeleteAsync(ResourceBlock item)
    {
        await base.DeleteAsync(item);
        await UpdateCache(item);
    }

    private async Task<List<ResourceBlock>> GetResourceBlockByEmployeeId(int employeeId, int year, int week)
    {
        var sql = "SELECT * FROM ResourceBlock WHERE EmployeeId = @employeeId AND Year = @year AND Week = @week";
        return await QueryAsync(sql, new { employeeId, year, week });
    }

    private async Task<List<ResourceBlock>> GetResourceBlockByProjectId(int projectId, int year, int week)
    {
        var sql = "SELECT * FROM ResourceBlock WHERE ProjectId = @projectId AND Year = @year AND Week = @week";
        return await QueryAsync(sql, new { projectId, year, week });
    }

    public async Task CopyProjectResourceBlocksFromWeek(int projectId, int copyToYear, int copyToWeek, int copyFromYear, int copyFromWeek)
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

    public async Task CopyEmployeeResourceBlocksFromWeek(int employeeId, int copyToYear, int copyToWeek, int copyFromYear, int copyFromWeek)
    {
        var sql = "SELECT * FROM ResourceBlock WHERE EmployeeId = @employeeId AND Year = @copyFromYear AND Week = @copyFromWeek";
        List<ResourceBlock> items = await QueryAsync(sql, new { employeeId, copyFromYear, copyFromWeek });

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
