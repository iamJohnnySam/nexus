using DataModels.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class ReviewPoint
{
    [Key]
    public int ReviewPointId { get; set; }
    public int ModuleId { get; set; }
    public ProductModule? Module { get; set; }
    public string ReviewCategory { get; set; } =  "General";
    public string ReviewDescription { get; set; } = string.Empty;

    public static TableMetadata Metadata => new(
        typeof(ReviewPoint).Name,
        new Dictionary<string, EDataType>
        {
                { nameof(ReviewPointId), EDataType.Key },
                { nameof(ModuleId), EDataType.Integer },
                { nameof(ReviewCategory), EDataType.Text },
                { nameof(ReviewDescription), EDataType.Text }
        },
        nameof(ReviewDescription)
    );
}
