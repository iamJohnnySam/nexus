using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutSimulator.Creator;

public class StationStruct
{
    public string FriendlyName { get; set; } = "Untitled Station";
    public string Identifier { get; set; } = "S";
    public int ProductModuleId { get; set; }
    public required string PayloadType { get; set; }
    public string ProcessIdsCSV { get; set; } = string.Empty;
    public List<uint> ProcessIds
    {
        get
        {
            return [.. ProcessIdsCSV.Split(",").Select(uint.Parse)];
        }
        set
        {
            ProcessIdsCSV = String.Join(",", value);
        }
    }
    public string AccessibleLocationsWithDoorCSV { get; set; } = string.Empty;
    public List<string> AccessibleLocationsWithDoor
    {
        get
        {
            return [.. AccessibleLocationsWithDoorCSV.Split(",")];
        }
        set
        {
            AccessibleLocationsWithDoorCSV = String.Join(",", value);
        }
    }
    public string AccessibleLocationsWithoutDoorCSV { get; set; } = string.Empty;
    public List<string> AccessibleLocationsWithoutDoor
    {
        get
        {
            return [.. AccessibleLocationsWithoutDoorCSV.Split(",")];
        }
        set
        {
            AccessibleLocationsWithoutDoorCSV = String.Join(",", value);
        }
    }
    public string DoorTransitionTimesCSV { get; set; } = string.Empty;
    public List<uint> DoorTransitionTimes
    {
        get
        {
            return [.. DoorTransitionTimesCSV.Split(",").Select(uint.Parse)];
        }
        set
        {
            DoorTransitionTimesCSV = String.Join(",", value);
        }
    }
    public string AccessiblePayloadsThroughtGapCSV { get; set; } = string.Empty;
    public List<int> AccessiblePayloadsThroughtGap
    {
        get
        {
            return [.. AccessiblePayloadsThroughtGapCSV.Split(",").Select(int.Parse)];
        }
        set
        {
            AccessiblePayloadsThroughtGapCSV = String.Join(",", value);
        }
    }
    public string AccessiblePayloadsThroughDoorCSV { get; set; } = string.Empty;
    public List<int> AccessiblePayloadsThroughDoor
    {
        get
        {
            return [.. AccessiblePayloadsThroughDoorCSV.Split(",").Select(int.Parse)];
        }
        set
        {
            AccessiblePayloadsThroughDoorCSV = String.Join(",", value);
        }
    }
    public int Capacity { get; set; } = 1;
    public bool Processable { get; set; } = false;
    public bool IsIndexable { get; set; } = false;
    public uint IndexTimePerSlot { get; set; } = 1;
    public bool IsInputAndPodDockable { get; set; } = false;
    public bool IsOutputAndPodDockable { get; set; } = false;
    public bool HighPriority { get; set; } = false;
    public int SimulationCommandSpecificationId { get; set; }
    public int Count { get; set; } = 1;
}
