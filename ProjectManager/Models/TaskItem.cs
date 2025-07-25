﻿using System;
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
    public Employee? Responsible { get; set; }
    public int ResponsibleId { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsBlocking { get; set; } = true; // Is blocking the timeline and not a background task
    public int? ParentTaskId { get; set; }
    public bool PriorityTask { get; set; } = false;
}
