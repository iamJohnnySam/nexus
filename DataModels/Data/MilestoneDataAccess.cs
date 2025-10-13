using DataModels.DataTools;
using DataModels.Tools;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class MilestoneDataAccess(string connectionString, EmployeeDataAccess employeeDB, ProjectDataAccess projectDB, ProjectStageDataAccess projectStageDB) : DataAccess<Milestone>(connectionString, Milestone.Metadata)
{
    private readonly EmployeeDataAccess EmployeeDB = employeeDB;
    private readonly ProjectDataAccess ProjectDB = projectDB;
    private readonly ProjectStageDataAccess ProjectStageDB = projectStageDB;

    private async Task GetObjects(Milestone? milestone)
    {
        if (milestone != null)
        {
            milestone.Project = await ProjectDB.GetByIdAsync(milestone.ProjectId);
            if (milestone.EngineerId != 0)
            {
                milestone.Engineer = await EmployeeDB.GetByIdAsync(milestone.EngineerId);
            }
            if (milestone.ProjectStageId != 0)
            {
                milestone.ProjectStage = await ProjectStageDB.GetByIdAsync(milestone.ProjectStageId);
            }
        }
    }

    public async override Task<List<Milestone>> GetAllAsync(string? orderBy = null, bool descending = false)
    {
        var milestones = await base.GetAllAsync(orderBy, descending);
        foreach (var milestone in milestones)
        {
            await GetObjects(milestone);
        }
        return milestones;
    }

    public async Task<List<Milestone>> GetByProjectIdAsync(int id)
    {
        var milestones = await GetByColumnAsync(nameof(Milestone.ProjectId), id);

        foreach (Milestone milestone in milestones)
        {
            await GetObjects(milestone);
        }

        return [.. milestones
            .OrderBy(m => m.StartDate)
            .ThenBy(m => m.StartDate.AddDays(m.RequiredDays))];
    }
    public async Task<List<Milestone>> GetActiveMilestonesForEngineer(int engineerId)
    {
        string sql = @"
        SELECT * FROM Milestone
        WHERE EngineerId = @EngineerId
          AND DATE(StartDate) IS NOT NULL
          AND DATE(StartDate, '+' || RequiredDays || ' days') > DATE('now');";

        var milestones = await QueryAsync(sql, new
        {
            EngineerId = engineerId
        });

        foreach (Milestone m in milestones)
        {
            await GetObjects(m);
        }

        return [.. milestones
            .OrderBy(m => m.StartDate)
            .ThenBy(m => m.StartDate.AddDays(m.RequiredDays))];
    }
    public async Task FixMilestoneStartDates(int id)
    {
        List<Milestone> milestones = await GetByProjectIdAsync(id);
        var milestoneDict = milestones.ToDictionary(m => m.MilestoneId);
        var dependencyGraph = milestones
            .GroupBy(m => m.DependentMilestoneId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var resolved = new HashSet<int>();
        var queue = new Queue<Milestone>();

        foreach (Milestone milestone in milestones.Where(m => m.DependentMilestoneId == 0))
        {
            AddMilestoneMissingDates(milestone);
            resolved.Add(milestone.MilestoneId);
            queue.Enqueue(milestone);
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!dependencyGraph.ContainsKey(current.MilestoneId))
                continue;

            foreach (var dependent in dependencyGraph[current.MilestoneId])
            {
                if (resolved.Contains(dependent.MilestoneId)) continue;

                switch (dependent.DependencyType)
                {
                    case EDependencyType.FinishToStart:
                        dependent.StartDate = CalendarLogic.AddWorkDays(current.StartDate, current.RequiredDays);
                        dependent.PlannedStartDate = CalendarLogic.AddWorkDays(current.PlannedStartDate, current.PlannedRequiredDays);
                        break;

                    case EDependencyType.StartToStart:
                        dependent.StartDate = current.StartDate;
                        dependent.PlannedStartDate = current.PlannedStartDate;
                        break;

                    case EDependencyType.FinishToFinish:
                        dependent.StartDate = CalendarLogic.AddWorkDays(current.StartDate, current.RequiredDays - dependent.RequiredDays);
                        dependent.PlannedStartDate = CalendarLogic.AddWorkDays(current.PlannedStartDate, current.PlannedRequiredDays - dependent.PlannedRequiredDays);
                        break;

                    case EDependencyType.StartToFinish:
                        dependent.StartDate = CalendarLogic.AddWorkDays(current.StartDate, -dependent.RequiredDays);
                        dependent.PlannedStartDate = CalendarLogic.AddWorkDays(current.PlannedStartDate, -dependent.PlannedRequiredDays);
                        break;
                }

                AddMilestoneMissingDates(dependent);
                _ = UpdateAsync(dependent);

                resolved.Add(dependent.MilestoneId);
                queue.Enqueue(dependent);
            }
        }
    }
    public static void AddMilestoneMissingDates(Milestone milestone)
    {
        milestone.EndDate = CalendarLogic.AddWorkDays(milestone.StartDate, milestone.RequiredDays);
        milestone.PlannedEndDate = CalendarLogic.AddWorkDays(milestone.PlannedStartDate, milestone.PlannedRequiredDays);
    }
    public async Task<List<Milestone>> GetMilestonesForProjectBetweenDates(int projectId, DateTime windowStart, DateTime windowEnd)
    {
        string sql = @"
                SELECT * FROM Milestone
                WHERE ProjectId = @ProjectId
                AND DATE(StartDate) IS NOT NULL;";

        var milestones = await QueryAsync(sql, new
        {
            ProjectId = projectId,
            StartDate = windowStart.ToString("yyyy-MM-dd"),
            EndDate = windowEnd.ToString("yyyy-MM-dd"),
        });

        List<Milestone> RelevantMilestones = [];

        foreach (Milestone milestone in milestones)
        {
            DateTime milestoneStart = milestone.StartDate;
            DateTime milestoneEnd = CalendarLogic.AddWorkDays(milestoneStart, milestone.RequiredDays);

            bool sceneA = milestoneStart >= windowStart && milestoneStart < windowEnd;
            bool sceneB = milestoneEnd >= windowStart && milestoneEnd < windowEnd;
            bool sceneC = windowStart >= milestoneStart && windowStart <= milestoneEnd;

            if (sceneA || sceneB || sceneC)
            {
                await GetObjects(milestone);
                RelevantMilestones.Add(milestone);
            }

        }

        return RelevantMilestones;
    }
    public async Task<List<Milestone>> GetAllMilestonesOfBlockingEngineers(int engineerId, DateTime startDate, DateTime endDate, int excludeProjectId)
    {
        string sql = @"
        SELECT * FROM Milestone
        WHERE EngineerId = @EngineerId
          AND ProjectId != @ExcludeProjectId
          AND DATE(StartDate) IS NOT NULL;";

        var milestones = await QueryAsync(sql, new
        {
            EngineerId = engineerId,
            ExcludeProjectId = excludeProjectId,
            StartDate = startDate.ToString("yyyy-MM-dd"),
            EndDate = endDate.ToString("yyyy-MM-dd")
        });

        List<Milestone> RelevantMilestones = [];

        foreach (Milestone milestone in milestones)
        {
            DateTime milestoneStart = milestone.StartDate;
            DateTime milestoneEnd = CalendarLogic.AddWorkDays(milestoneStart, milestone.RequiredDays);

            bool sceneA = milestoneStart >= startDate && milestoneStart < endDate;
            bool sceneB = milestoneEnd >= startDate && milestoneEnd < endDate;
            bool sceneC = startDate >= milestoneStart && startDate <= milestoneEnd;

            if (sceneA || sceneB || sceneC)
            {
                await GetObjects(milestone);
                RelevantMilestones.Add(milestone);
            }

        }

        return RelevantMilestones;
    }
}
