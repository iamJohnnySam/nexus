using DataModels.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class ResourceBlock
{
    public int ResourceBlockId { get; set; }
    public int EmployeeId { get; set; }
    public int ProjectId { get; set; }
    public int Year { get; set; }
    public int Week { get; set; }

    public static TableMetadata Metadata => new(
        typeof(ResourceBlock).Name,
        new Dictionary<string, EDataType>
        {
                { nameof(ResourceBlockId), EDataType.Key },
                { nameof(EmployeeId), EDataType.Integer },
                { nameof(ProjectId), EDataType.Integer },
                { nameof(Year), EDataType.Integer },
                { nameof(Week), EDataType.Integer }
        },
        nameof(EmployeeId)
    );
}
