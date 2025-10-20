using DataModels.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class Milestone
{
    public int MilestoneId { get; set; }
    public string MilestoneName { get; set; } = string.Empty;
    public string Comments { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public DateTime PlannedDate { get; set; }
    public DateTime ActualDate { get; set; }
    public bool IsCompleted { get; set; } = false;
    public bool OnHold { get; set; } = false;
    public int ResponsibleId { get; set; }

    public static TableMetadata Metadata => new(
        typeof(Milestone).Name,
        new Dictionary<string, EDataType>
        {
            { nameof(MilestoneId), EDataType.Key },
            { nameof(MilestoneName), EDataType.Text },
            { nameof(Comments), EDataType.Text },
            { nameof(ProjectId), EDataType.Integer },
            { nameof(PlannedDate), EDataType.Date },
            { nameof(ActualDate), EDataType.Date },
            { nameof(IsCompleted), EDataType.Boolean },
            { nameof(OnHold), EDataType.Boolean },
            { nameof(ResponsibleId), EDataType.Integer }
        },
        nameof(PlannedDate)
    );
}
