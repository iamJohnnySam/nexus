using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutSimulator.Commands;

public class Job
{
    public required ECommandType Action { get; set; }
    public required string TransactionID { get; set; }
    public required string Target { get; set; }
    public Dictionary<int, string> Arguments { get; set; } = [];
    public required string RawAction { get; set; }
    public required string RawCommand { get; set; }
}
