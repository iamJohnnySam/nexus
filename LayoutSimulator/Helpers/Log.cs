using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutSimulator.Helpers;

public sealed class Log
{
    public event EventHandler<LogMessage>? OnLogEvent;

    private static Log? instance = null;

    private static readonly object padlock = new();

    private Log()
    {
        
    }

    public static Log Instance
    {
        get
        {
            lock (padlock)
            {
                instance ??= new Log();
                return instance;
            }
        }
    }

    public void Info(LogMessage message)
    {
        OnLogEvent?.Invoke(this, message);
    }
}