using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager.Models;

public class ProjectBlock
{
    [Key]
    public int ProjectBlockId { get; set; }
    public int ProjectId { get; set; }
    public int ProjectPhaseId { get; set; } = 0;
    public int Year { get; set; }
    public int Week { get; set; }
}
