using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager.Models;

public class Supplier
{
    [Key]
    public int SupplierId { get; set; }
    public required string SupplierName { get; set; }
    public string SupplierWebsite { get; set; } = string.Empty;
    public string SupplierContact { get; set; } = string.Empty;
    public List<string> ProductTypes { get; set; } = [];
}
