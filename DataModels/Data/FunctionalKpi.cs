using DataModels.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class FunctionalKpi
{
    [Key]
    public int FunctionalKpiId { get; set; }
    public required string KpiName { get; set; }
    public string KpiDescription { get; set; } = string.Empty;
    public required string KpiDepartment { get; set; } = "Engineering Design";
    public int KpiEffectiveFrom { get; set; } = DateTime.Now.Year;

    public static TableMetadata Metadata => new(
        typeof(FunctionalKpi).Name,
        new Dictionary<string, EDataType>
        {
                { nameof(FunctionalKpiId), EDataType.Key },
                { nameof(KpiName), EDataType.Text },
                { nameof(KpiDescription), EDataType.Text },
                { nameof(KpiDepartment), EDataType.Text },
                { nameof(KpiEffectiveFrom), EDataType.Date }
        },
        nameof(KpiName)
    );
}
