using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutModels.Creator;

public class CStation
{
    public required string Identifier { get; set; }
    public string PayloadType { get; set; } = "type1";
    public int Capacity { get; set; }
    public List<CProcess> Processes { get; set; } = [];
    public List<string> AccessibleLocationsWithDoor { get; set; } = [];
    public List<string> AccessibleLocationsWithoutDoor { get; set; } = [];
    public List<int> DoorTransitionTimes { get; set; } = [];
    public bool ConcurrentLocationAccess { get; set; }
    public bool Processable { get; set; }
    public int ProcessTime { get; set; }
    public bool PodDockable { get; set; }
    public bool AutoLoadPod { get; set; } = true;
    public bool AutoDoorControl { get; set; }
    public bool LowPriority { get; set; }
    public bool PartialProcess { get; set; } = false;
    public int Count { get; set; }
    public List<string> AcceptedCommands { get; set; } = [];
}
