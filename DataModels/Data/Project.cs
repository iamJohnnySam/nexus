using DataModels.Tools;
using System.ComponentModel.DataAnnotations;

namespace DataModels.Data;

public class Project
{
    [Key]
    public int ProjectId { get; set; }
    public required string ProjectName { get; set; }
    public int CustomerId { get; set; }
    public string? DesignCode { get; set; }
    public string? ProjectCode { get; set; }
    public EProjectPriority Priority { get; set; } = EProjectPriority.Normal;
    public ESalesStatus POStatus { get; set; }
    public int ProductId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsTrackedProject { get; set; } = true;
    public int PrimaryDesignerId { get; set; }
    public string RequirementDocumentLink { get; set; } = string.Empty;

    public Customer? Customer { get; set; }
    public Product? Product { get; set; }
    public Employee? PrimaryDesigner { get; set; }

    public static TableMetadata Metadata => new(
        typeof(Project).Name,
        new Dictionary<string, EDataType>
        {
            { nameof(ProjectId), EDataType.Key },
            { nameof(ProjectName), EDataType.Text },
            { nameof(CustomerId), EDataType.Integer },
            { nameof(DesignCode), EDataType.Text },
            { nameof(ProjectCode), EDataType.Text },
            { nameof(Priority), EDataType.Integer },
            { nameof(POStatus), EDataType.Integer },
            { nameof(ProductId), EDataType.Integer },
            { nameof(IsActive), EDataType.Integer },
            { nameof(IsTrackedProject), EDataType.Integer },
            { nameof(PrimaryDesignerId), EDataType.Integer },
            { nameof(RequirementDocumentLink), EDataType.Text }
        },
        nameof(ProjectName)
    );
}
