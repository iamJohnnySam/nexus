using ProjectManager.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusModels;

public class Module
{
    [Key]
    public int ModuleId { get; set; }
    public required string ModuleName { get; set; }
    public List<Project> Projects { get; set; } = [];
}
