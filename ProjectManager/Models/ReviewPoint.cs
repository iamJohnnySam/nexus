using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager.Models;

public class ReviewPoint
{
    [Key]
    public int ReviewPointId { get; set; }
    public int ModuleId { get; set; }
    public required string ReviewDescription { get; set; }
}
