using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutSimulator.Commands;

public enum EResponseType
{
    Ack = 0,
    Nack = 1,
    Success = 2,
    Error = 3
}
