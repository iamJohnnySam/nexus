using DataModels.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class ProjectStage
{
    [Key]
    public int ProjectStageId { get; set; }
    public string ProjectStageName { get; set; } = "Untitled Project Stage";
    public string ProjectStageDescription { get; set; } = string.Empty;
    public int Sequence { get; set; }

    public static TableMetadata Metadata => new(
        typeof(ProjectStage).Name,
        new Dictionary<string, EDataType>
        {
            { nameof(ProjectStageId), EDataType.Key },
            { nameof(ProjectStageName), EDataType.Text },
            { nameof(ProjectStageDescription), EDataType.Text },
            { nameof(Sequence), EDataType.Integer }
        },
        nameof(ProjectStageName)
    );
}
