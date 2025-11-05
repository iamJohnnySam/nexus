using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutSimulator.Commands;

public class CommandReply
{
    public required EResponseType ResponseType { get; set; }
    public required string Response { get; set; }
}
