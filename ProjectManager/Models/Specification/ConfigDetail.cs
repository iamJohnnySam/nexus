using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager.Models;

public class ConfigDetail
{
    [Key]
    public int ConfigDetailId { get; set; }
    public int ConfigurationId { get; set; }
    public Configuration? Configuration { get; set; }
    public int SpecificationId { get; set; }
    public Specification? Specification { get; set; }
    public required string SpecificationDetail { get; set; }
    public string Comments { get; set; } = string.Empty;
    public int Revision { get; set; } = 0;
    public DateTime FirstAdded { get; set; } = DateTime.Now;
    public DateTime LastUpdated { get; set; } = DateTime.Now;
}
