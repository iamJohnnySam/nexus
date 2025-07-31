using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager.Models;

public class ResourceBlock
{
    public int ResourceBlockId { get; set; }
    public int EmployeeId { get; set; }
    public int ProjectId { get; set; }
    public int Year { get; set; }
    public int Week { get; set; }
}
