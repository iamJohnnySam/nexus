using DataModels.DataTools;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class EmployeeDataAccess(string connectionString, GradeDataAccess gradeDB, DesignationDataAccess designationDB) : DataAccess<Employee>(connectionString, Employee.Metadata)
{
    private GradeDataAccess GradeDB { get; } = gradeDB;
    private DesignationDataAccess DesignationDB { get; } = designationDB;

    private async Task GetObjects(Employee? emp)
    {
        if (emp != null)
        {
            emp.EmployeeGrade = await GradeDB.GetByIdAsync(emp.GradeId);
            emp.EmployeeDesignation = await DesignationDB.GetByIdAsync(emp.DesignationId);
        }
    }

    public async Task<List<Employee>> GetAllActiveEmployees()
    {
        await FixActiveEmployees();
        var employees = await GetByColumnAsync(nameof(Employee.IsActive), true);

        foreach (var emp in employees)
        {
            await GetObjects(emp);
        }

        List<Employee> groupedAndSorted = [.. employees
            .OrderBy(e => e.EmployeeDesignation.DesignationName)
            .ThenByDescending(e => e.EmployeeGrade.GradeScore)];

        return groupedAndSorted;
    }
    public async Task FixActiveEmployees()
    {
        List<Employee> employees = await GetAllAsync();
        var today = DateTime.Today;

        foreach (var emp in employees)
        {
            bool status;
            if (emp.JoinDate > today || (emp.LeaveDate.HasValue && emp.LeaveDate.Value < today))
            {
                status = false;
            }
            else
            {
                status = true;
            }
            if (status != emp.IsActive)
            {
                emp.IsActive = status;
                await UpdateAsync(emp);
            }
        }
    }

    public async override Task<Employee?> GetByIdAsync(object id)
    {
        Employee emp = await base.GetByIdAsync(id);
        await GetObjects(emp);

        return emp;
    }
    public async Task<List<Employee>> GetAllActiveEmployeesByDesignationId(int id)
    {
        List<Employee> employees = await QueryAsync("SELECT * FROM Employee WHERE IsActive = 1 AND DesignationId = @id", new { id });

        foreach (var emp in employees)
        {
            await GetObjects(emp);
        }

        List<Employee> groupedAndSorted = [.. employees
            .OrderBy(e => e.EmployeeDesignation.DesignationName)
            .ThenByDescending(e => e.EmployeeGrade.GradeScore)];

        return groupedAndSorted;
    }
}
