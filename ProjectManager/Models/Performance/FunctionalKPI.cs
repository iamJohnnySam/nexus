using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager.Models;

public class FunctionalKPI
{
    [Key]
    public int FunctionalKPIId { get; set; }
    public required string KPIName { get; set; }
    public string KPIDescription { get; set; } = string.Empty;
    public required string KPIDepartment { get; set; } = "Engineering Design";
    public int KPIEffectiveFrom { get; set; } = DateTime.Now.Year;
}
