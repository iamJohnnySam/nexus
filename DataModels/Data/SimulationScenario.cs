using DataModels.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class SimulationScenario
{
    [Key]
    public int SimulationScenarioId { get; set; }
    public required string SimulationName { get; set; }
    public int ProjectId { get; set; }
    public required string XMLFile { get; set; }
    public float LastThroughput { get; set; }

    public static TableMetadata Metadata => new(
        typeof(SimulationScenario).Name,
        new Dictionary<string, EDataType>
        {
                { nameof(SimulationScenarioId), EDataType.Key },
                { nameof(SimulationName), EDataType.Text },
                { nameof(ProjectId), EDataType.Integer },
                { nameof(XMLFile), EDataType.LongText },
                { nameof(LastThroughput), EDataType.Float }
        },
        nameof(SimulationName)
    );
}
