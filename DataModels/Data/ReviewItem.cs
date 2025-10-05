using DataModels.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class ReviewItem
{
    [Key]
    public int ReviewItemId { get; set; }
    public int ProjectId { get; set; }
    public int ReviewPointId { get; set; }
    public bool Approved { get; set; } = false;
    public bool NotApplicable { get; set; } = false;
    public DateTime? LastReviewDate { get; set; }
    public string ReviewComments { get; set; } = string.Empty;
    public int ReviewResponsibleID { get; set; }

    public static TableMetadata Metadata => new(
        typeof(ReviewItem).Name,
        new Dictionary<string, EDataType>
        {
                { nameof(ReviewItemId), EDataType.Key },
                { nameof(ProjectId), EDataType.Integer },
                { nameof(ReviewPointId), EDataType.Integer },
                { nameof(Approved), EDataType.Boolean },
                { nameof(NotApplicable), EDataType.Boolean },
                { nameof(LastReviewDate), EDataType.Date },
                { nameof(ReviewComments), EDataType.Text },
                { nameof(ReviewResponsibleID), EDataType.Integer }
        },
        nameof(ReviewItemId)
    );

}
