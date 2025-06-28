using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager.Models;

public class Project
{
    [Key]
    public int ProjectId { get; set; }
    public required string ProjectName { get; set; }
    public int CustomerID { get; set; }
    public string? DesignCode { get; set; }
    public List<string> PreviousCodes { get; set; } = [];
    public List<string> POCodes { get; set; } = [];
    public ProjectPriority Priority { get; set; } = ProjectPriority.Normal;
    public SalesStatus POStatus { get; set; }
    public int ProductId { get; set; }
}
