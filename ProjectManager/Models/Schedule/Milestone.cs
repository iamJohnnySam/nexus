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
    public DateTime PlannedStartDate { get; set; } = DateTime.Now;
    public DateTime? EndDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }
    public int RequiredDays { get; set; }
    public int PlannedRequiredDays { get; set; }

    public int DependentMilestoneId { get; set; }
    public DependencyType DependencyType { get; set; } = DependencyType.FinishToStart;
    public int EngineerId { get; set; }
    public Employee? Engineer { get; set; } = null;
    public bool IsCompleted { get; set; } = false;
    public int ProjectStageId { get; set; }
    public ProjectStage? ProjectStage { get; set; }
}
