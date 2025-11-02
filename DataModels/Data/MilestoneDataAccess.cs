using DataModels.DataTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class MilestoneDataAccess(string connectionString) : DataAccess<Milestone>(connectionString, Milestone.Metadata)
{
    private Dictionary<int, MilestoneProjectLink> ProjectLinks { get; set; } = [];

    public async Task<MilestoneProjectLink> GetProjectLink(int ProjectId)
    {
        if (!ProjectLinks.ContainsKey(ProjectId))
        {
            ProjectLinks[ProjectId] = new MilestoneProjectLink(ProjectId);
            await UpdateProjectMilestones(ProjectId, true);
            await UpdateProjectMilestones(ProjectId, false);
        }
        return ProjectLinks[ProjectId];
    }

    private async Task<List<Milestone>> GetByProjectIdAsync(int projectId)
    {
        return await GetByColumnAsync("ProjectId", projectId);
    }
    private async Task<List<Milestone>> GetOpenByProjectIdAsync(int projectId)
    {
        string sql = @"SELECT * FROM Milestone WHERE IsCompleted = 0 AND ProjectId = @ProjectId;";
        return await QueryAsync(sql, new { ProjectId = projectId });
    }  
    private async Task<List<Milestone>> GetCompletedByProjectIdAsync(int projectId)
    {
        string sql = @"SELECT * FROM Milestone WHERE IsCompleted = 1 AND ProjectId = @ProjectId;";
        return await QueryAsync(sql, new { ProjectId = projectId });
    }

    public override async Task InsertAsync(Milestone milestone)
    {
        await base.InsertAsync(milestone);
        await UpdateProjectMilestones(milestone.ProjectId, milestone.IsCompleted);
    }
    public override async Task UpdateAsync(Milestone milestone)
    {
        bool completionBefore = (await GetByIdAsync(milestone.ProjectId))!.IsCompleted;
        await base.UpdateAsync(milestone);
        if (completionBefore != milestone.IsCompleted)
            await UpdateProjectMilestones(milestone.ProjectId, completionBefore);
        await UpdateProjectMilestones(milestone.ProjectId, milestone.IsCompleted);
    }
    public override async Task DeleteAsync(Milestone milestone)
    {
        await base.DeleteAsync(milestone);
        await UpdateProjectMilestones(milestone.ProjectId, milestone.IsCompleted);
    }

    private async Task UpdateProjectMilestones(int ProjectId, bool completed)
    {
        if (ProjectLinks.ContainsKey(ProjectId))
        {
            if (completed)
            {
                ProjectLinks[ProjectId].CompletedItems = (await GetCompletedByProjectIdAsync(ProjectId)).OrderByDescending(planned => planned.ActualDate).ToList(); ;
            }
            else
            {
                ProjectLinks[ProjectId].IncompleteItems = await GetOpenByProjectIdAsync(ProjectId);
            }
        }
        OnPropertyChanged();
    }
}
