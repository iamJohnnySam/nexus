using DataModels.Tools;
using LayoutSimulator.Creator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class SimulationStation : StationStruct
{
    public int SimulationStationId { get; set; }

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
            { nameof(Processable), EDataType.Boolean },
            { nameof(IndexTimePerSlot), EDataType.Integer },
            { nameof(IsInputAndPodDockable), EDataType.Boolean },
            { nameof(IsOutputAndPodDockable), EDataType.Boolean },
            { nameof(HighPriority), EDataType.Boolean },
            { nameof(SimulationCommandSpecificationId), EDataType.Integer }
        },
        nameof(FriendlyName)
    );
}
