using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager.Models;

public class Module
{
    [Key]
    public int ModuleId { get; set; }
    public required string ModuleName { get; set; }
}
