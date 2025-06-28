using NexusModels.Enums;
using ProjectManager.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusModels.ProjectReview;

public class Review
{
    [Key]
    public int ReviewId { get; set; }
    public int ProjectId { get; set; }
    public required Project Project { get; set; }
    public int ReviewPointId { get; set; }
    public required ReviewPoint ReviewPoint { get; set; }
    public int ReviewItemId { get; set; }
    public required ProjectReviewItem ReviewItem { get; set; }
}
