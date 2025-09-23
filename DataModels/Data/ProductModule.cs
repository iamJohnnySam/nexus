using DataModels.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class ProductModule
{
    [Key]
    public int ModuleId { get; set; }
    public required string ModuleName { get; set; }
    public int Rank { get; set; }

    public static TableMetadata Metadata => new(
        typeof(ProductModule).Name,
        new Dictionary<string, EDataType>
        {
                { nameof(ModuleId), EDataType.Key },
                { nameof(ModuleName), EDataType.Text },
                { nameof(Rank), EDataType.Integer }
        },
        nameof(ModuleName)
    );
}
