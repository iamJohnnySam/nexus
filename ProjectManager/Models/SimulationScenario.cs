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
    public required string SimulationName { get; set; }
    public int ProjectId { get; set; }
    public required string XMLFile { get; set; }
    public float LastThroughput { get; set; }
}
