using DataModels.Administration;
using DataModels.DataTools;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class TaskItemDataAccess(string connectionString, EmployeeDataAccess employeeDB) : DataAccess<TaskItem>(connectionString, TaskItem.Metadata)
{
    private readonly EmployeeDataAccess EmployeeDB = employeeDB;

    public TaskItem GetNewParentTask(LoginInformation LoginInfo)
    {
        return new TaskItem
        {
            Title = "Untitled Task",
            ProjectId = LoginInfo.CurrentProject.ProjectId,
            Deadline = DateTime.Now,
            ResponsibleId = 0
        };
    }
    public TaskItem GetNewSubTask(TaskItem t, LoginInformation LoginInfo)
    {
        return new TaskItem
        {
            Title = "Untitled Task",
            ProjectId = LoginInfo.CurrentProject.ProjectId,
            Deadline = DateTime.Now,
            ParentTaskId = t.TaskId,
            ResponsibleId = 0
        };
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
    public async Task<List<TaskItem>> GetAllParentTasks(int projectID)
    {
        string query = "SELECT * FROM TaskItem WHERE ProjectId = @ProjectId AND (ParentTaskId IS NULL OR ParentTaskId = 0)";
        return await QueryTaskList(query, new { ProjectId = projectID });
    }
    public async Task<List<TaskItem>> GetAllIncompleteParentTasks(int projectID)
    {
        string query = "SELECT * FROM TaskItem WHERE ProjectId = @ProjectId AND (ParentTaskId IS NULL OR ParentTaskId = 0) AND IsCompleted = 0";
        return await QueryTaskList(query, new { ProjectId = projectID });
    }
    public async Task<List<TaskItem>> GetAllCompleteParentTasks(int projectID)
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
    public async Task<List<TaskItem>> GetAllSubTasksOfParentTask(int parentTaskId)
    {
        string query = "SELECT * FROM TaskItem WHERE ParentTaskId = @parentTaskId";
        return await QueryTaskList(query, new { parentTaskId });
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
        if(p.Deadline < p.StartedOn)
        {
            p.Deadline = p.StartedOn;
        }
        await base.UpdateAsync(p);
    }
}
