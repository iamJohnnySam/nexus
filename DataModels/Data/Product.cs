using DataModels.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class Product
{
    [Key]
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "Untitled Product";

    public static TableMetadata Metadata => new(
        typeof(Product).Name,
        new Dictionary<string, EDataType>
        {
            { nameof(ProductId), EDataType.Key },
            { nameof(ProductName), EDataType.Text }
        },
        nameof(ProductName)
    );
}
