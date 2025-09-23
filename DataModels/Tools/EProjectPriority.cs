using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Tools;

public enum EProjectPriority
{
    [Description("Not Started")]
    NotStarted,
    Low,
    Normal,
    High,
    Completed,
    Discarded
}
