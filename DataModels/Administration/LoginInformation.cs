using DataModels.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Administration;

public class LoginInformation
{
    public Project? CurrentProject { get; set; }
    public int CurrentEmployeeId { get; set; } = 0;
    public Employee? CurrentEmployee { get; set; }
    public bool LoggedIn { get; set; } = false;
}
