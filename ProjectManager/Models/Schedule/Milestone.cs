using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager.Models;

public class Milestone
{
    [Key]
    public int MilestoneId { get; set; }

    public int ProjectId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public List<MilestoneDependency> Dependencies { get; set; } = new();
}
