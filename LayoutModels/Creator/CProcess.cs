using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutModels.Creator;

public class CProcess
{
    public string? ProcessName { get; set; }
    public string InputState { get; set; } = string.Empty;
    public string OutputState { get; set; } = string.Empty;
    public string? Location { get; set; }
    public int ProcessTime { get; set; }
}
