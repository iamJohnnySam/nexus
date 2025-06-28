using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusModels;

public class Product
{
    [Key]
    public int ProductId { get; set; }
    public required string ProductName { get; set; }
}
