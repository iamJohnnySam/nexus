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
    public string ModuleName { get; set; } = "Untitled Module";
    public EModuleType ModuleType { get; set; } = EModuleType.None;
    public int Rank { get; set; }

    public static TableMetadata Metadata => new(
        typeof(ProductModule).Name,
        new Dictionary<string, EDataType>
        {
                { nameof(ModuleId), EDataType.Key },
                { nameof(ModuleType), EDataType.Integer },
                { nameof(ModuleName), EDataType.Text },
                { nameof(Rank), EDataType.Integer }
        },
        nameof(Rank)
    );
}
