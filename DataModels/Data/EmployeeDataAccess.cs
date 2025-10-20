using DataModels.DataTools;
using DataModels.Tools;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class EmployeeDataAccess(string connectionString, GradeDataAccess gradeDB, DesignationDataAccess designationDB) : DataAccess<Employee>(connectionString, Employee.Metadata)
{
    public  bool ActiveEmployeesFixed { get; set; }
    private GradeDataAccess GradeDB { get; } = gradeDB;
    private DesignationDataAccess DesignationDB { get; } = designationDB;

    private async Task GetObjects(Employee? emp)
    {
        if (emp != null)
        {
            emp.EmployeeGrade = await GradeDB.GetByIdAsync(emp.GradeId);
            emp.EmployeeDesignation = await DesignationDB.GetByIdAsync(emp.DesignationId);
            if (emp.ReplacedEmployeeId != 0)
                emp.ReplacedEmployee = await GetByIdAsync(emp.ReplacedEmployeeId);
            if (emp.LineManagerId != 0)
                emp.LineManager = await GetByIdAsync(emp.LineManagerId);
        }
    }

    public async Task<List<Employee>> GetAllActiveAsync()
    {
        await FixActiveEmployees(true);
        var employees = await GetByColumnAsync(nameof(Employee.IsActive), true);

        foreach (var emp in employees)
        {
            await GetObjects(emp);
        }

        List<Employee> groupedAndSorted = [.. employees
            .OrderBy(e => e.EmployeeDesignation!.DesignationName)
            .ThenByDescending(e => e.EmployeeGrade!.GradeScore)];

        return groupedAndSorted;
    }

    public async Task<List<Employee>> GetAllEmployeesActiveWithin(int year, int WeekNumber)
    {
        await FixActiveEmployees();
        DateTime startOfTheWeek = CalendarLogic.GetFirstMondayOfWeek(year, WeekNumber);

        string sqlQ = "SELECT * FROM Employee WHERE @startOfTheWeek BETWEEN JoinDate AND LeaveDate OR (LeaveDate IS NULL AND @startOfTheWeek >= JoinDate);";
        return await QueryAsync(sqlQ, new { startOfTheWeek });
    }

    public async Task FixActiveEmployees(bool byPass = false)
    {
        if (!ActiveEmployeesFixed || byPass)
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
            .OrderBy(e => e.EmployeeDesignation!.DesignationName)
            .ThenByDescending(e => e.EmployeeGrade!.GradeScore)];

        return groupedAndSorted;
    }
}
