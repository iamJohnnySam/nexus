using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutSimulator.Models;

public class Pod()
{
    public required string PodID { get; set; }
    public required Cassette Cassette { get; set; }
}
