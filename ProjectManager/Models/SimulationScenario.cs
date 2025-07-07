using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager.Models;

public class SimulationScenario
{
    [Key]
    public int SimulationScenarioId { get; set; }
    public string SimulationName { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public string XMLFile { get; set; } = string.Empty;
}
