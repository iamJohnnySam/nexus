using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutSimulator;

public class LogMessage
{
    public string? transactionID;
    public string message;

    public LogMessage(string tID, string msg)
    {
        transactionID = tID;
        message = msg;
    }

    public LogMessage(string msg)
    {
        message = msg;
    }

}
