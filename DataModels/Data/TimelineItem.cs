using DataModels.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class TimelineItem
{
    [Key]
    public int TimelineItemId { get; set; }
    public required int ProjectId { get; set; }
    public Project? Project { get; set; } = null;

    public string Name { get; set; } = "Untitled Milestone";

    public DateTime StartDate { get; set; }
    public DateTime PlannedStartDate { get; set; } = DateTime.Now;
    public DateTime? EndDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }
    public int RequiredDays { get; set; }
    public int PlannedRequiredDays { get; set; }

    public int DependentTimelineItemId { get; set; }
    public EDependencyType DependencyType { get; set; } = EDependencyType.FinishToStart;
    public int EngineerId { get; set; }
    public Employee? Engineer { get; set; } = null;
    public bool IsCompleted { get; set; } = false;
    public int ProjectStageId { get; set; }
    public ProjectStage? ProjectStage { get; set; }

    public static TableMetadata Metadata => new(
        typeof(TimelineItem).Name,
        new Dictionary<string, EDataType>
        {
            { nameof(TimelineItemId), EDataType.Key },
            { nameof(ProjectId), EDataType.Integer },
            { nameof(Name), EDataType.Text },
            { nameof(StartDate), EDataType.Date },
            { nameof(PlannedStartDate), EDataType.Date },
            { nameof(EndDate), EDataType.Date },
            { nameof(PlannedEndDate), EDataType.Date },
            { nameof(RequiredDays), EDataType.Integer },
            { nameof(PlannedRequiredDays), EDataType.Integer },
            { nameof(DependentTimelineItemId), EDataType.Integer },
            { nameof(DependencyType), EDataType.Integer },
            { nameof(EngineerId), EDataType.Integer },
            { nameof(IsCompleted), EDataType.Boolean },
            { nameof(ProjectStageId), EDataType.Integer }
        },
        nameof(StartDate)
    );
}
