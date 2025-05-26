using NexusModels;
using NexusModels.Enums;
using NexusModels.ProjectReview;
using NexusModels.ProjectTasks;
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
    public string Customer { get; set; } = "Mindox Techno";
    public string? DesignCode { get; set; }
    public string? SalesCode { get; set; }
    public string? POCode { get; set; }
    public ProjectPriority Priority { get; set; } = ProjectPriority.Normal;
    public SalesStatus POStatus { get; set; }
    public int ProductCategoryId { get; set; }
    public required Product ProductCategory { get; set; }

    public List<Module> Modules { get; set; } = [];
    public List<TaskItem> TaskItems { get; set; } = [];
    public List<Review> Reviews { get; set; } = [];

}
