using NexusModels.People;
using ProjectManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NexusModels.ProjectTasks;

public class TaskItem
{
    [Key]
    public int TaskId { get; set; }
    public int ProjectId { get; set; }
    public required Project Project { get; set; }
    public required string Title { get; set; }
    public List<Employee> Responsible { get; set; } = [];
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public required DateTime Deadline { get; set; }
    public bool IsCompleted { get; set; }
    public int? ParentTaskItemId { get; set; }
    public TaskItem? ParentTaskItem { get; set; }

    public List<TaskItem> SubTasks { get; set; } = [];

    public bool IsFullyCompleted => IsCompleted && SubTasks.All(s => s.IsFullyCompleted);

}
