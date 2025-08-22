using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager.Models;

public class Specification
{
    [Key]
    public int SpecificationId { get; set; }
    public required string SpecificationName { get; set; }
    public string SpecificationDescription { get; set; } = string.Empty;
    public int ProductModuleId { get; set; }
    public ProductModule? ProductModule { get; set; }
    public string ConfigurationOptions {  get; set; } = string.Empty;
}
