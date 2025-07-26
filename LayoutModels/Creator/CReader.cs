using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutModels.Creator;

public class CReader
{
    public required string Identifier { get; set; }
    public required string StationID { get; set; }
    public int Slot { get; set; } = 0;
}
