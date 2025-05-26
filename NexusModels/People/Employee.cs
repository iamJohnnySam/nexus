using NexusModels.ProjectTasks;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusModels.People;

public class Employee
{
    [Key]
    public int EmployeeId { get; set; }
    public required string EmployeeName { get; set; }
    public int? EmployeeGradeId { get; set; }
    public Grade? EmployeeGrade { get; set; }
    public int? EmployeeDesignationId { get; set; }
    public Designation? EmployeeDesignation { get; set; }
    public DateTime EmployeeJoinDate { get; set; }
    public bool IsActive { get; set; } = true;
    public List<TaskItem> Tasks { get; set; } = [];
}
