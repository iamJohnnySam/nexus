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
    NotStarted = 0,
    Low = 3,
    Normal = 4,
    High = 5,
    Completed = 2,
    Discarded = 1
}
