using DataModels.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class MilestoneTemplate
{
    public int MilestoneTemplateId { get; set; }
    public string MilestoneName { get; set; } = string.Empty;
    public int Days { get; set; }
    public bool IsSelected { get; set; }

    public static TableMetadata Metadata => new(
        typeof(MilestoneTemplate).Name,
        new Dictionary<string, EDataType>
        {
            { nameof(MilestoneTemplateId), EDataType.Key },
            { nameof(MilestoneName), EDataType.Text },
            { nameof(Days), EDataType.Integer }
        },
        nameof(Days)
    );
}
