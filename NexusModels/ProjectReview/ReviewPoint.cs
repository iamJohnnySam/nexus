using NexusModels.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusModels.ProjectReview;

public class ReviewPoint
{
    [Key]
    public int ReviewPointId { get; set; }
    public int ModuleUnderTestId { get; set; }
    public required Module ModuleUnderTest { get; set; }
    public required string ReviewDescription { get; set; }
    public List<Review> Reviews { get; set; } = [];
}
