using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager.Models.Enums;

public enum DependencyType
{
    StartAfterEnd, // Default: Task B starts after Task A ends
    StartWith,     // Task B starts when Task A starts
    EndWith        // Task B ends when Task A ends
}
