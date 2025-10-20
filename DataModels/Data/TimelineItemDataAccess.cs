using DataModels.DataTools;
using DataModels.Tools;

namespace DataModels.Data;

public class TimelineItemDataAccess(string connectionString, EmployeeDataAccess employeeDB, ProjectDataAccess projectDB, ProjectStageDataAccess projectStageDB) : DataAccess<TimelineItem>(connectionString, TimelineItem.Metadata)
{
    private readonly EmployeeDataAccess EmployeeDB = employeeDB;
    private readonly ProjectDataAccess ProjectDB = projectDB;
    private readonly ProjectStageDataAccess ProjectStageDB = projectStageDB;

    private async Task GetObjects(TimelineItem? timelineItem)
    {
        if (timelineItem != null)
        {
            timelineItem.Project = await ProjectDB.GetByIdAsync(timelineItem.ProjectId);
            if (timelineItem.EngineerId != 0)
            {
                timelineItem.Engineer = await EmployeeDB.GetByIdAsync(timelineItem.EngineerId);
            }
            if (timelineItem.ProjectStageId != 0)
            {
                timelineItem.ProjectStage = await ProjectStageDB.GetByIdAsync(timelineItem.ProjectStageId);
            }
        }
    }

    public async override Task<List<TimelineItem>> GetAllAsync(string? orderBy = null, bool descending = false)
    {
        var timelineItems = await base.GetAllAsync(orderBy, descending);
        foreach (var timelineItem in timelineItems)
        {
            await GetObjects(timelineItem);
        }
        return timelineItems;
    }

    public async Task<List<TimelineItem>> GetByProjectIdAsync(int id)
    {
        var timelineItems = await GetByColumnAsync(nameof(TimelineItem.ProjectId), id);

        foreach (TimelineItem timelineItem in timelineItems)
        {
            await GetObjects(timelineItem);
        }

        return [.. timelineItems
            .OrderBy(m => m.StartDate)
            .ThenBy(m => m.StartDate.AddDays(m.RequiredDays))];
    }
    public async Task<List<TimelineItem>> GetActiveTimelineItemForEngineer(int engineerId)
    {
        string sql = @"
        SELECT * FROM TimelineItem
        WHERE EngineerId = @EngineerId
          AND DATE(StartDate) IS NOT NULL
          AND DATE(StartDate, '+' || RequiredDays || ' days') > DATE('now');";

        var timelineItems = await QueryAsync(sql, new
        {
            EngineerId = engineerId
        });

        foreach (TimelineItem m in timelineItems)
        {
            await GetObjects(m);
        }

        return [.. timelineItems
            .OrderBy(m => m.StartDate)
            .ThenBy(m => m.StartDate.AddDays(m.RequiredDays))];
    }
    public async Task FixTimelineItemStartDates(int id)
    {
        List<TimelineItem> timelineItems = await GetByProjectIdAsync(id);
        var timelineItemDict = timelineItems.ToDictionary(m => m.TimelineItemId);
        var dependencyGraph = timelineItems
            .GroupBy(m => m.DependentTimelineItemId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var resolved = new HashSet<int>();
        var queue = new Queue<TimelineItem>();

        foreach (TimelineItem timelineItem in timelineItems.Where(m => m.DependentTimelineItemId == 0))
        {
            AddTimelineItemMissingDates(timelineItem);
            resolved.Add(timelineItem.TimelineItemId);
            queue.Enqueue(timelineItem);
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!dependencyGraph.ContainsKey(current.TimelineItemId))
                continue;

            foreach (var dependent in dependencyGraph[current.TimelineItemId])
            {
                if (resolved.Contains(dependent.TimelineItemId)) continue;

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

                AddTimelineItemMissingDates(dependent);
                _ = UpdateAsync(dependent);

                resolved.Add(dependent.TimelineItemId);
                queue.Enqueue(dependent);
            }
        }
    }
    public static void AddTimelineItemMissingDates(TimelineItem milestone)
    {
        milestone.EndDate = CalendarLogic.AddWorkDays(milestone.StartDate, milestone.RequiredDays);
        milestone.PlannedEndDate = CalendarLogic.AddWorkDays(milestone.PlannedStartDate, milestone.PlannedRequiredDays);
    }
    public async Task<List<TimelineItem>> GetTimelineItemForProjectBetweenDates(int projectId, DateTime windowStart, DateTime windowEnd)
    {
        string sql = @"
                SELECT * FROM TimelineItem
                WHERE ProjectId = @ProjectId
                AND DATE(StartDate) IS NOT NULL;";

        var timelineItems = await QueryAsync(sql, new
        {
            ProjectId = projectId,
            StartDate = windowStart.ToString("yyyy-MM-dd"),
            EndDate = windowEnd.ToString("yyyy-MM-dd"),
        });

        List<TimelineItem> RelevantTimelineItems = [];

        foreach (TimelineItem timelineItem in timelineItems)
        {
            DateTime milestoneStart = timelineItem.StartDate;
            DateTime milestoneEnd = CalendarLogic.AddWorkDays(milestoneStart, timelineItem.RequiredDays);

            bool sceneA = milestoneStart >= windowStart && milestoneStart < windowEnd;
            bool sceneB = milestoneEnd >= windowStart && milestoneEnd < windowEnd;
            bool sceneC = windowStart >= milestoneStart && windowStart <= milestoneEnd;

            if (sceneA || sceneB || sceneC)
            {
                await GetObjects(timelineItem);
                RelevantTimelineItems.Add(timelineItem);
            }

        }

        return RelevantTimelineItems;
    }
    public async Task<List<TimelineItem>> GetAllTimelineItemsOfBlockingEngineers(int engineerId, DateTime startDate, DateTime endDate, int excludeProjectId)
    {
        string sql = @"
        SELECT * FROM TimelineItem
        WHERE EngineerId = @EngineerId
          AND ProjectId != @ExcludeProjectId
          AND DATE(StartDate) IS NOT NULL;";

        var timelineItems = await QueryAsync(sql, new
        {
            EngineerId = engineerId,
            ExcludeProjectId = excludeProjectId,
            StartDate = startDate.ToString("yyyy-MM-dd"),
            EndDate = endDate.ToString("yyyy-MM-dd")
        });

        List<TimelineItem> RelevantTimelineItems = [];

        foreach (TimelineItem timelineItem in timelineItems)
        {
            DateTime milestoneStart = timelineItem.StartDate;
            DateTime milestoneEnd = CalendarLogic.AddWorkDays(milestoneStart, timelineItem.RequiredDays);

            bool sceneA = milestoneStart >= startDate && milestoneStart < endDate;
            bool sceneB = milestoneEnd >= startDate && milestoneEnd < endDate;
            bool sceneC = startDate >= milestoneStart && startDate <= milestoneEnd;

            if (sceneA || sceneB || sceneC)
            {
                await GetObjects(timelineItem);
                RelevantTimelineItems.Add(timelineItem);
            }

        }

        return RelevantTimelineItems;
    }
}
