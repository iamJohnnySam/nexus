﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager.Models;

public enum SalesStatus
{
    [Description("Concept Stage")]
    Concept,
    [Description("PO Project")]
    POProject,
    [Description("After Sales")]
    AfterSales,
    InternalProject
}
