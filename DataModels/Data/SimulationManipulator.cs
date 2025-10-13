using DataModels.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class SimulationManipulator
{
    public int SimulationManipulatorId { get; set; }
    public string ManipulatorName { get; set; } = "Untitled Manipulator";
    public string ManipulatorIdentifier { get; set; } = "M";
    public int ProductModuleId { get; set; }
    private string EndEffectorsCSV { get; set; } = string.Empty;
    public required List<string> EndEffectors
    {
        get
        {
            return [.. EndEffectorsCSV.Split(",")];
        }
        set
        {
            EndEffectorsCSV = String.Join(",", value);
        }
    }
    private string LocationsCSV { get; set; } = string.Empty;
    public required List<string> Locations
    {
        get
        {
            return [.. LocationsCSV.Split(",")];
        }
        set
        {
            LocationsCSV = String.Join(",", value);
        }
    }
    public int MotionTime { get; set; }
    public int ExtendTime { get; set; }
    public int RetractTime { get; set; }

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
