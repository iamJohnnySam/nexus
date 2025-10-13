using DataModels.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class Customer
{
    [Key]
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = "New Customer";

    public static TableMetadata Metadata => new(
        typeof(Customer).Name,
        new Dictionary<string, EDataType>
        {
                { nameof(CustomerId), EDataType.Key },
                { nameof(CustomerName), EDataType.Text }
        },
        nameof(CustomerName)
    );
}
