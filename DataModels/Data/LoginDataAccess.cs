using DataModels.DataTools;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class LoginDataAccess(string connectionString) : DataAccess<Login>(connectionString, Login.Metadata)
{
    public async Task<List<Login>> GetAllActive()
    {
        return await GetByColumnAsync(nameof(Login.IsActive), true);
    }
    public async Task<Login?> GetByEmployeeId(int employeeId)
    {
        return await GetOneByColumnAsync(nameof(Login.EmployeeId), employeeId);
    }
}
