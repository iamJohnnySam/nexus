
using DataModels.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class Grade
{
    [Key]
    public int GradeId { get; set; }
    public required string GradeName { get; set; }
    public int GradeScore { get; set; }

    public static TableMetadata Metadata => new(
        typeof(Grade).Name,
        new Dictionary<string, EDataType>
        {
            { nameof(GradeId), EDataType.Key },
            { nameof(GradeName), EDataType.Text },
            { nameof(GradeScore), EDataType.Integer }
        },
        nameof(GradeName)
    );
}
