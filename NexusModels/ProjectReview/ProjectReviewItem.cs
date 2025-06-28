using ProjectManager.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusModels.ProjectReview;

public class ProjectReviewItem
{
    [Key]
    public int ReviewItemId { get; set; }
    public int ProjectId { get; set; }
    public required Project Project {  get; set; }
    public bool Approved { get; set; } = false;
    public DateTime? LastReviewDate { get; set; }
    public string ReviewComments { get; set; } = string.Empty;

    public List<Review> Reviews { get; set; } = [];

}
