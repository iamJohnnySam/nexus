using DataModels.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class Configuration
{
    [Key]
    public int ConfigurationId { get; set; }
    public string ConfigurationName { get; set; } = "New Configuration";
    public string ConfigurationDescription { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public Project? Project { get; set; } = null;
    public int ProductModuleId { get; set; }
    public ProductModule? ProductModule { get; set; } = null;
    public int Quantity { get; set; } = 1;
    public bool IsAddOn { get; set; }
    public bool IsRequired { get; set; }

    public static TableMetadata Metadata => new(
        typeof(Configuration).Name,
        new Dictionary<string, EDataType>
        {
                { nameof(ConfigurationId), EDataType.Key },
                { nameof(ConfigurationName), EDataType.Text },
                { nameof(ConfigurationDescription), EDataType.Text },
                { nameof(ProjectId), EDataType.Integer },
                { nameof(ProductModuleId), EDataType.Integer },
                { nameof(Quantity), EDataType.Integer },
                { nameof(IsAddOn), EDataType.Boolean },
                { nameof(IsRequired), EDataType.Boolean }
        },
        nameof(ConfigurationName)
    );
}
