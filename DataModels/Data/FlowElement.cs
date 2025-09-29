using DataModels.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class FlowElement
{
    public int FlowElementId { get; set; }
    public required string ElementName { get; set; }
    public string ElementType { get; set; } = "Process";
    public int PrecedingId { get; set; } = 0;
    public int RevertId { get; set; } = 0;
    public double X { get; set; } = 100;
    public double Y { get; set; } = 100;
    public static TableMetadata Metadata => new(
        typeof(FlowElement).Name,
        new Dictionary<string, EDataType>
        {
                { nameof(FlowElementId), EDataType.Key },
                { nameof(ElementName), EDataType.Text },
                { nameof(ElementType), EDataType.Text },
                { nameof(PrecedingId), EDataType.Integer },
                { nameof(RevertId), EDataType.Integer }
        },
        nameof(PrecedingId)
    );
}
