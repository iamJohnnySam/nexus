using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutSimulator.Creator;

public class ProcessStruct
{
    public int ProcessId { get; set; }
    public string ProcessName { get; set; } = "Untitled Process";
    public string? InputState { get; set; }
    public string? OutputState { get; set; }
    public string? NextLocation { get; set; }
    public uint ProcessTime { get; set; } = 1;
}
