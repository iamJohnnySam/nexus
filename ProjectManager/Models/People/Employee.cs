using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager.Models;

public class Employee
{
    [Key]
    public int EmployeeId { get; set; }
    public required string EmployeeName { get; set; }
    public Grade EmployeeGrade { get; set; }
    public int GradeId { get; set; }
    public Designation EmployeeDesignation { get; set; }
    public int DesignationId { get; set; }
    public DateTime JoinDate { get; set; }
    public DateTime? LeaveDate { get; set; }
    public required bool IsActive { get; set; }
    public int ReplacedEmployeeId { get; set; }
}
