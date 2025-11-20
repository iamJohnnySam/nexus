using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutSimulator.Creator;

public class ManipulatorStruct
{
    public string ManipulatorName { get; set; } = "Untitled Manipulator";
    public string ManipulatorIdentifier { get; set; } = "M";
    public int ProductModuleId { get; set; }
    public string EndEffectorsCSV { get; set; } = string.Empty;
    public List<string> EndEffectors
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
    public string EndEffectorSlotsCSV { get; set; } = string.Empty;
    public List<uint> EndEffectorSlots
    {
        get
        {
            return [.. EndEffectorSlotsCSV.Split(",").Select(uint.Parse)];
        }
        set
        {
            EndEffectorSlotsCSV = String.Join(",", value);
        }
    }
    public string LocationsCSV { get; set; } = string.Empty;
    public List<string> Locations
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
    public int Count { get; set; } = 1;
}
