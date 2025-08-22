using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager.Models;

public class Configuration
{
    [Key]
    public int ConfigurationId { get; set; }
    public required string ConfigurationName { get; set; }
    public string ConfigurationDescription { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public Project? Project { get; set; }
    public int ProductModuleId { get; set; }
    public ProductModule? ProductModule { get; set; }
    public required int Quantity { get; set; } = 1;
    public bool IsAddOn { get; set; }
    public bool IsRequired { get; set; }
}
