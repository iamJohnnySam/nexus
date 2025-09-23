using DataModels.Data;
using DataModels.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class ProjectBlock
{
    [Key]
    public int ProjectBlockId { get; set; }
    public int ProjectId { get; set; }
    public int ProjectPhaseId { get; set; } = 0;
    public int Year { get; set; }
    public int Week { get; set; }

    public static TableMetadata Metadata => new(
        typeof(ProjectBlock).Name,
        new Dictionary<string, EDataType>
        {
                { nameof(ProjectBlockId), EDataType.Key },
                { nameof(ProjectId), EDataType.Integer },
                { nameof(ProjectPhaseId), EDataType.Integer },
                { nameof(Year), EDataType.Integer },
                { nameof(Week), EDataType.Integer }
        },
        nameof(ProjectPhaseId)
    );
}
