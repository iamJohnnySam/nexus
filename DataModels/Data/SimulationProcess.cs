using DataModels.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class SimulationProcess
{
    public int SimulationProcessId { get; set; }
    public required string ProcessName { get; set; }
    public string? InputState { get; set; }
    public string? OutputState { get; set; }
    public string? Location { get; set; }
    public int ProcessTime { get; set; } = 1;
    public static TableMetadata Metadata => new(
        typeof(SimulationProcess).Name,
        new Dictionary<string, EDataType>
        {
            { nameof(SimulationProcessId), EDataType.Key },
            { nameof(ProcessName), EDataType.Text },
            { nameof(InputState), EDataType.Text },
            { nameof(OutputState), EDataType.Text },
            { nameof(Location), EDataType.Text },
            { nameof(ProcessTime), EDataType.Integer }
        },
        nameof(ProcessName)
    );
}
