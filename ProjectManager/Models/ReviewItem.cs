using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager.Models;

public class ReviewItem
{
    [Key]
    public int ReviewItemId { get; set; }
    public int ProjectId { get; set; }
    public int ReviewPointId { get; set; }
    public bool Approved { get; set; } = false;
    public DateTime? LastReviewDate { get; set; }
    public string ReviewComments { get; set; } = string.Empty;
    public int ReviewResponsibleID { get; set; }

}
