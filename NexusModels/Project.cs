using NexusModels.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager.Models
{
    public class Project
    {
        [Key]
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Customer { get; set; } = "Mindox Techno";
        public string? DesignCode { get; set; }
        public string? SalesCode { get; set; }
        public string? POCode { get; set; }
        public ProjectPriority Priority { get; set; } = ProjectPriority.Normal;
        public SalesStatus POStatus { get; set; }
        public ProductCategory ProductCategory { get; set; }

    }
}
