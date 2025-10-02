using DataModels.DataTools;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class SimulationScenarioDataAccess(string connectionString) : DataAccess<SimulationScenario>(connectionString, SimulationScenario.Metadata)
{
    public async Task<List<SimulationScenario>> GetByProjectId(int projectId)
    {
        return await GetByColumnAsync(nameof(SimulationScenario.ProjectId), projectId);
    }
}
