using DataModels.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class Designation
{
    [Key]
    public int DesignationId { get; set; }
    public string DesignationName { get; set; } = "Untitled Designation";
    public string? Department {  get; set; }

    public static TableMetadata Metadata => new(
        typeof(Designation).Name,
        new Dictionary<string, EDataType>
        {
            { nameof(DesignationId), EDataType.Key },
            { nameof(DesignationName), EDataType.Text },
            { nameof(Department), EDataType.Text }
        },
        nameof(DesignationName)
    );
}
