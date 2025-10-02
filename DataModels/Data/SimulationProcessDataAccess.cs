using DataModels.DataTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class SimulationProcessDataAccess(string connectionString) : DataAccess<SimulationProcess>(connectionString, SimulationProcess.Metadata)
{
}
