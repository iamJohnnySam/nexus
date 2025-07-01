using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager.Models;

public class TaskItem
{
    [Key]
    public int TaskId { get; set; }
    public int ProjectId { get; set; }
    public required string Title { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public DateTime StartedOn {  get; set; } = DateTime.Now;
    public required DateTime Deadline { get; set; }
    public bool IsCompleted { get; set; }
    public int? ParentTaskId { get; set; }

}
