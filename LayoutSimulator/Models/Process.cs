using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutSimulator.Models;

public class Process
{
    public required string ProcessName { get; set; }
    public required string InputState { get; set; }
    public required string OutputState { get; set; }
    public required string NextLocation { get; set; }
    public float ProcessTime { get; set; }
}
