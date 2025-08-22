using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager.Models;

public class OEMItem
{
    [Key]
    public int OEMItemId { get; set; }
    public required ProductModule ProductModule { get; set; }
    public int ProductModuleId { get; set; }
    public required Supplier Supplier { get; set; }
    public int SupplierId { get; set; }
    public required string OEMName { get; set; }
    public required string OEMPartNumber { get; set; }
    public string OEMDescription { get; set; } = string.Empty;
    public string InternalPartNumber { get; set; } = string.Empty;
    public string KitItems { get; set; } = string.Empty;
}
