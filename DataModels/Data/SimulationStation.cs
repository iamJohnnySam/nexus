using DataModels.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class SimulationStation
{
    public int SimulationStationId { get; set; }
    public string FriendlyName { get; set; } = "Untitled Station";
    public string Identifier { get; set; } = "S";
    public int ProductModuleId { get; set; }
    public required string PayloadType { get; set; }
    private string ProcessIdsCSV { get; set; } = string.Empty;
    public required List<uint> ProcessIds
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
    public List<SimulationProcess> Processes { get; set; } = [];
    public int Capacity { get; set; } = 1;
    private string AccessibleLocationsWithDoorCSV { get; set; } = string.Empty;
    public required List<string> AccessibleLocationsWithDoor { 
        get
        {
            return [.. AccessibleLocationsWithDoorCSV.Split(",")];
        }
        set 
        {
            AccessibleLocationsWithDoorCSV = String.Join(",", value); 
        }
    }
    private string AccessibleLocationsWithoutDoorCSV { get; set; } = string.Empty;
    public required List<string> AccessibleLocationsWithoutDoor { 
        get
        {
            return [.. AccessibleLocationsWithoutDoorCSV.Split(",")];
        }
        set 
        {
            AccessibleLocationsWithoutDoorCSV = String.Join(",", value); 
        }
    }
    private string DoorTransitionTimesCSV { get; set; } = string.Empty;
    public required List<uint> DoorTransitionTimes
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
    public bool ConcurrentLocationAccess { get; set; } = false;
    public bool Processable { get; set; } = false;
    public bool SingleSlotAccess { get; set; } = false;
    public uint ProcessTime { get; set; } = 1;
    public uint IndexTimePerSlot { get; set; } = 1;
    public bool PodDockable { get; set; } = false;
    public bool AutoPodLoad { get; set; } = false;
    public bool AutoDoorControl { get; set; } = false;
    public bool LowPriority { get; set; } = false;
    public int SimulationCommandSpecificationId { get; set; }

    public static TableMetadata Metadata => new(
        typeof(SimulationStation).Name,
        new Dictionary<string, EDataType>
        {
            { nameof(SimulationStationId), EDataType.Key },
            { nameof(FriendlyName), EDataType.Text },
            { nameof(Identifier), EDataType.Text },
            { nameof(ProductModuleId), EDataType.Integer },
            { nameof(PayloadType), EDataType.Text },
            { nameof(ProcessIdsCSV), EDataType.Text },
            { nameof(Capacity), EDataType.Text },
            { nameof(AccessibleLocationsWithDoorCSV), EDataType.Text },
            { nameof(AccessibleLocationsWithoutDoorCSV), EDataType.Text },
            { nameof(DoorTransitionTimesCSV), EDataType.Text },
            { nameof(ConcurrentLocationAccess), EDataType.Boolean },
            { nameof(Processable), EDataType.Boolean },
            { nameof(ProcessTime), EDataType.Integer },
            { nameof(SingleSlotAccess), EDataType.Boolean },
            { nameof(IndexTimePerSlot), EDataType.Integer },
            { nameof(PodDockable), EDataType.Boolean },
            { nameof(AutoPodLoad), EDataType.Boolean },
            { nameof(AutoDoorControl), EDataType.Boolean },
            { nameof(LowPriority), EDataType.Boolean },
            { nameof(SimulationCommandSpecificationId), EDataType.Integer }
        },
        nameof(FriendlyName)
    );
}
