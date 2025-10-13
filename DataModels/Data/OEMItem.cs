using DataModels.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class OEMItem
{
    [Key]
    public int OEMItemId { get; set; }
    public required int ProductModuleId { get; set; }
    public ProductModule? ProductModule { get; set; }
    public required int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public string OEMName { get; set; } = "Untitled OEM Name";
    public string OEMPartNumber { get; set; } = string.Empty;
    public string OEMDescription { get; set; } = string.Empty;
    public string InternalPartNumber { get; set; } = string.Empty;
    public string KitItems { get; set; } = string.Empty;

    public static TableMetadata Metadata => new(
        typeof(OEMItem).Name,
        new Dictionary<string, EDataType>
        {
                { nameof(OEMItemId), EDataType.Key },
                { nameof(ProductModuleId), EDataType.Integer },
                { nameof(SupplierId), EDataType.Integer },
                { nameof(OEMName), EDataType.Text },
                { nameof(OEMPartNumber), EDataType.Text },
                { nameof(OEMDescription), EDataType.Text },
                { nameof(InternalPartNumber), EDataType.Text },
                { nameof(KitItems), EDataType.Text }
        },
        nameof(OEMName)
    );
}
