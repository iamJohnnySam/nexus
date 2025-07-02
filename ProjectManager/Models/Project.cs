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
    public int CustomerId { get; set; }
    public string? DesignCode { get; set; }
    public ProjectPriority Priority { get; set; } = ProjectPriority.Normal;
    public SalesStatus POStatus { get; set; }
    public int ProductId { get; set; }

    public Customer? Customer { get; set; }
    public Product? Product { get; set; }
}
