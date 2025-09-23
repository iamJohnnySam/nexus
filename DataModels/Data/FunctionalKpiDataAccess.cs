using DataModels.DataTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class FunctionalKpiDataAccess(string connectionString) : DataAccess<FunctionalKpi>(connectionString, FunctionalKpi.Metadata)
{
}
