using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager.Models;

public class ProjectStage
{
    [Key]
    public int ProjectStageId { get; set; }
    public required string ProjectStageName { get; set; }
    public required string ProjectStageDescription { get; set; }
    public int Sequence { get; set; }
}
