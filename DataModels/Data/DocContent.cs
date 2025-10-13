using DataModels.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class DocContent
{
    public int DocContentId { get; set; }
    public int DocTypeId { get; set; }
    public DocType? DocumentType { get; set; }
    public string SectionType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;


    public static TableMetadata Metadata => new(
        typeof(DocContent).Name,
        new Dictionary<string, EDataType>
        {
                { nameof(DocContentId), EDataType.Key },
                { nameof(Content), EDataType.Text }
        },
        nameof(DocContentId)
    );
}
