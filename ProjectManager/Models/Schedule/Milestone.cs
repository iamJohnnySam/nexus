using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager.Models;

public class Milestone
{
    [Key]
    public int MilestoneId { get; set; }
    public required int ProjectId { get; set; }
    public Project? Project { get; set; } = null;

    public required string Name { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int RequiredDays { get; set; }

    public int DependentMilestoneId { get; set; }
    public DependencyType DependencyType { get; set; } = DependencyType.FinishToStart;
    public int EngineerId { get; set; }
    public Employee? Engineer { get; set; } = null;
    public bool IsCompleted { get; set; } = false;
}
