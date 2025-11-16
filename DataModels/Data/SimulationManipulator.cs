using DataModels.Tools;
using LayoutSimulator.Creator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class SimulationManipulator : ManipulatorStruct
{
    public int SimulationManipulatorId { get; set; }
    

    public static TableMetadata Metadata => new(
        typeof(SimulationManipulator).Name,
        new Dictionary<string, EDataType>
        {
            { nameof(SimulationManipulatorId), EDataType.Key },
            { nameof(ManipulatorName), EDataType.Text },
            { nameof(ManipulatorIdentifier), EDataType.Text },
            { nameof(ProductModuleId), EDataType.Integer },
            { nameof(EndEffectorsCSV), EDataType.Text },
            { nameof(LocationsCSV), EDataType.Text },
            { nameof(MotionTime), EDataType.Integer },
            { nameof(ExtendTime), EDataType.Integer },
            { nameof(RetractTime), EDataType.Integer }
        },
        nameof(ManipulatorName)
    );
}
