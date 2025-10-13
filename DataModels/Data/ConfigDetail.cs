using DataModels.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class ConfigDetail
{
    [Key]
    public int ConfigDetailId { get; set; }
    public int ConfigurationId { get; set; }
    public Configuration? Configuration { get; set; } = null;
    public int SpecificationId { get; set; }
    public Specification? Specification { get; set; } = null;
    public string SpecificationDetail { get; set; } = string.Empty;
    public string Comments { get; set; } = string.Empty;
    public int Revision { get; set; } = 0;
    public DateTime FirstAdded { get; set; } = DateTime.Now;
    public DateTime LastUpdated { get; set; } = DateTime.Now;

    public static TableMetadata Metadata => new(
        typeof(ConfigDetail).Name,
        new Dictionary<string, EDataType>
        {
                { nameof(ConfigDetailId), EDataType.Key },
                { nameof(ConfigurationId), EDataType.Integer },
                { nameof(SpecificationId), EDataType.Integer },
                { nameof(SpecificationDetail), EDataType.Text },
                { nameof(Comments), EDataType.Text },
                { nameof(Revision), EDataType.Integer },
                { nameof(FirstAdded), EDataType.Date },
                { nameof(LastUpdated), EDataType.Date }
        },
        nameof(SpecificationDetail)
    );
}
