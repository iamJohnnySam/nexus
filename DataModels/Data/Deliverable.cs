using DataModels.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class Deliverable
{
    [Key]
    public int DeliverableId { get; set; }
    public string DeliverableName { get; set; } = "New Deliverable";
    public string DeliverableDescription { get; set; }= string.Empty;
    public string DeliverableType {  get; set; }= string.Empty;

    public static TableMetadata Metadata => new(
        typeof(Deliverable).Name,
        new Dictionary<string, EDataType>
        {
                { nameof(DeliverableId), EDataType.Key },
                { nameof(DeliverableName), EDataType.Text },
                { nameof(DeliverableDescription), EDataType.Text },
                { nameof(DeliverableType), EDataType.Text }
        },
        nameof(DeliverableName)
    );
}
