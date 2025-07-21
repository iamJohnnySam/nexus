using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutModels.Creator;

public class CManipulator
{
    public required string Identifier { get; set; }
    public List<string> EndEffectors { get; set; } = [];
    public List<string> Locations { get; set; } = [];
    public int MotionTime { get; set; }
    public int ExtendTime { get; set; }
    public int RetractTime { get; set; }
    public int Count { get; set; }
}
