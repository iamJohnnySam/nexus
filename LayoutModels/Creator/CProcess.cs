using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutModels.Creator;

public class CProcess
{
    public required string ProcessName { get; set; }
    public string InputState { get; set; } = string.Empty;
    public string OutputState { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int ProcessTime { get; set; }
}
