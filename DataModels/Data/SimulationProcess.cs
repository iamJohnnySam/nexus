using DataModels.Tools;
using LayoutSimulator.Creator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class SimulationProcess : ProcessStruct
{
    public static TableMetadata Metadata => new(
        typeof(SimulationProcess).Name,
        new Dictionary<string, EDataType>
        {
            { nameof(ProcessId), EDataType.Key },
            { nameof(ProcessName), EDataType.Text },
            { nameof(InputState), EDataType.Text },
            { nameof(OutputState), EDataType.Text },
            { nameof(NextLocation), EDataType.Text },
            { nameof(ProcessTime), EDataType.Integer }
        },
        nameof(ProcessName)
    );
}
