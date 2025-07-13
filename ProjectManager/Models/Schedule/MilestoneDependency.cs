using ProjectManager.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager.Models;

public class MilestoneDependency
{
    [Key]
    public int DependencyId { get; set; }

    public int MilestoneId { get; set; }         // The milestone this dependency belongs to
    public int DependsOnMilestoneId { get; set; } // The milestone it depends on
    public DependencyType Type { get; set; }
}
