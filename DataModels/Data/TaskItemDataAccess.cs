using DataModels.DataTools;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class TaskItemDataAccess(string connectionString) : DataAccess<TaskItem>(connectionString, TaskItem.Metadata)
{
    private readonly EmployeeDataAccess EmployeeDB = new(connectionString);

    private Dictionary<int, TaskItemProjectLink> ProjectLinks { get; set; } = [];
    private Dictionary<int, TaskItemParentTaskLink> ParentTaskLinks { get; set; } = [];

    public async Task<TaskItemProjectLink> GetProjectLink(int projectId)
    {
        if (!ProjectLinks.ContainsKey(projectId))
        {
            ProjectLinks[projectId] = new TaskItemProjectLink(projectId);
            await RefreshTasksForProjects(projectId, false, false);
            await RefreshTasksForProjects(projectId, false, true);
            await RefreshTasksForProjects(projectId, true, false);
            await RefreshTasksForProjects(projectId, true, true);
        }
        return ProjectLinks[projectId];
    }

    public async Task<TaskItemParentTaskLink> GetParentTaskLink(int parentTaskId)
    {
        if (!ParentTaskLinks.ContainsKey(parentTaskId))
        {
            ParentTaskLinks[parentTaskId] = new TaskItemParentTaskLink(parentTaskId);
            await RefreshTasksForParentTasks(parentTaskId);
        }
        return ParentTaskLinks[parentTaskId];
    }

    public TaskItem GetNewParentTask(int projectId)
    {
        return new TaskItem
        {
            Title = "Untitled Task",
            ProjectId = projectId,
            Deadline = DateTime.Now,
            ResponsibleId = 0
        };
    }
    public TaskItem GetNewSubTask(TaskItem t, int projectId)
    {
        return new TaskItem
        {
            Title = "Untitled Task",
            ProjectId = projectId,
            Deadline = DateTime.Now,
            ParentTaskId = t.TaskId,
            ResponsibleId = 0
        };
    }

    public override async Task InsertAsync(TaskItem t)
    {
        await base.InsertAsync(t);
        await RefreshTasksForProjects(t.ProjectId, t.ParentTaskId == 0, t.IsCompleted);
        await RefreshTasksForParentTasks(t.ParentTaskId ?? 0);
    }

    private async Task<List<TaskItem>> QueryTaskList(string query, object project)
    {
        var tasks = (await QueryAsync(query, project)).ToList();

        foreach (var task in tasks)
        {
            task.Responsible = await EmployeeDB.GetByIdAsync(task.ResponsibleId);
        }

        return tasks;
    }

    private async Task<List<TaskItem>> GetAllIncompleteParentTasks(int projectID)
    {
        string query = "SELECT * FROM TaskItem WHERE ProjectId = @ProjectId AND (ParentTaskId IS NULL OR ParentTaskId = 0) AND IsCompleted = 0";
        return await QueryTaskList(query, new { ProjectId = projectID });
    }
    private async Task<List<TaskItem>> GetAllCompleteParentTasks(int projectID)
    {
        string query = "SELECT * FROM TaskItem WHERE ProjectId = @ProjectId AND (ParentTaskId IS NULL OR ParentTaskId = 0) AND IsCompleted = 1";
        return await QueryTaskList(query, new { ProjectId = projectID });
    }
    private static Dictionary<int, List<TaskItem>> BundleSubTasks(List<TaskItem> AllTasks)
    {
        Dictionary<int, List<TaskItem>> SubTasks = [];
        foreach (TaskItem taskItem in AllTasks)
        {
            if (taskItem.ParentTaskId is null || taskItem.ParentTaskId == 0)
                continue;

            if (!SubTasks.ContainsKey(taskItem.ParentTaskId ?? 0))
            {
                SubTasks.Add(taskItem.ParentTaskId ?? 0, []);
            }

            SubTasks[taskItem.ParentTaskId ?? 0].Add(taskItem);
        }
        return SubTasks;
    }
    public async Task<Dictionary<int, List<TaskItem>>> GetAllSubTasks(int projectID)
    {
        string query = "SELECT * FROM TaskItem WHERE ProjectId = @ProjectId AND ParentTaskId IS NOT NULL AND ParentTaskId <> 0";
        List<TaskItem> AllTasks = await QueryTaskList(query, new { ProjectId = projectID });
        return BundleSubTasks(AllTasks);
    }
    private async Task<List<TaskItem>> GetAllSubTasksOfParentTask(int parentTaskId)
    {
        string query = "SELECT * FROM TaskItem WHERE ParentTaskId = @ParentTaskId";
        return await QueryTaskList(query, new { ParentTaskId = parentTaskId });
    }
    public async Task<Dictionary<int, List<TaskItem>>> GetAllCompleteSubTasks(int projectID)
    {
        string query = "SELECT * FROM TaskItem WHERE ProjectId = @ProjectId AND ParentTaskId IS NOT NULL AND ParentTaskId <> 0 AND IsCompleted = 1";
        List<TaskItem> AllTasks = await QueryTaskList(query, new { ProjectId = projectID });
        return BundleSubTasks(AllTasks);
    }
    public async Task<Dictionary<int, List<TaskItem>>> GetAllIncompleteSubTasks(int projectID)
    {
        string query = "SELECT * FROM TaskItem WHERE ProjectId = @ProjectId AND ParentTaskId IS NOT NULL AND ParentTaskId <> 0 AND IsCompleted = 0";
        List<TaskItem> AllTasks = await QueryTaskList(query, new { ProjectId = projectID });
        return BundleSubTasks(AllTasks);
    }
    public async Task MarkTaskComplete(TaskItem t)
    {
        t.IsCompleted = true;
        await UpdateTaskCompletion(t);
    }
    public async Task MarkTaskIncomplete(TaskItem t)
    {
        t.IsCompleted = false;
        await UpdateTaskCompletion(t);
    }
    private async Task UpdateTaskCompletion(TaskItem p)
    {
        string sql = @"UPDATE TaskItem
                       SET IsCompleted = @IsCompleted
                       WHERE TaskId = @TaskId;";
        await ExecuteAsync(sql, p);
    }
    public override async Task UpdateAsync(TaskItem p)
    {
        bool completionBefore = (await GetByIdAsync(p.TaskId))!.IsCompleted;
        if (p.Deadline < p.StartedOn)
        {
            p.Deadline = p.StartedOn;
        }
        bool update = false;
        if (completionBefore != p.IsCompleted)
        {
            update = true;
            if (p.IsCompleted && p.CompletedOn == null)
            {
                p.CompletedOn = DateTime.Now;
            }
        }

        await base.UpdateAsync(p);
        if (update)
            await RefreshTasksForProjects(p.ProjectId, p.ParentTaskId == 0, completionBefore);

        await RefreshTasksForProjects(p.ProjectId, p.ParentTaskId == 0, p.IsCompleted);
        await RefreshTasksForParentTasks(p.ParentTaskId ?? 0);
    }
    public override async Task DeleteAsync(TaskItem p)
    {
        await base.DeleteAsync(p);
        await RefreshTasksForProjects(p.ProjectId, p.ParentTaskId == 0, p.IsCompleted);
        await RefreshTasksForParentTasks(p.ParentTaskId ?? 0);
    }

    private async Task RefreshTasksForProjects(int projectId, bool parent, bool complete)
    {
        if(ProjectLinks.ContainsKey(projectId))
        {
            if (parent)
            {
                if (complete)
                {
                    ProjectLinks[projectId].CompletedParentTasks = await GetAllCompleteParentTasks(projectId);
                }
                else
                {
                    ProjectLinks[projectId].IncompleteParentTasks = await GetAllIncompleteParentTasks(projectId);
                }
            }
            else
            {
                ProjectLinks[projectId].SubTasks = await GetAllSubTasks(projectId);
            }
        }
    }

    private async Task RefreshTasksForParentTasks(int parentTaskId)
    {
        if (ParentTaskLinks.ContainsKey(parentTaskId))
        {
            ParentTaskLinks[parentTaskId].SubTasks = await GetAllSubTasksOfParentTask(parentTaskId);
        }
    }
}
