using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Tools;

public enum ESalesStatus
{
    [Description("Concept Stage")]
    Concept,
    [Description("PO Project")]
    POProject,
    [Description("After Sales")]
    AfterSales,
    [Description("Internal Project")]
    InternalProject
}
