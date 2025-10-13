using DataModels.Tools;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class DocType
{
    public int DocTypeId { get; set; }
    public string Name { get; set; } = "New Document Type";

    public static TableMetadata Metadata => new(
        typeof(DocType).Name,
        new Dictionary<string, EDataType>
        {
                { nameof(DocTypeId), EDataType.Key },
                { nameof(Name), EDataType.Text },
        },
        nameof(Name)
    );
}
