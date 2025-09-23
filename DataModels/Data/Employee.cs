using DataModels.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class Employee
{
    [Key]
    public int EmployeeId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public required string EmployeeName { get; set; }
    public Grade? EmployeeGrade { get; set; }
    public int GradeId { get; set; }
    public Designation? EmployeeDesignation { get; set; }
    public int DesignationId { get; set; }
    public DateTime JoinDate { get; set; }
    public DateTime? LeaveDate { get; set; }
    public required bool IsActive { get; set; }
    public int ReplacedEmployeeId { get; set; }

    public static TableMetadata Metadata => new(
        typeof(Employee).Name,
        new Dictionary<string, EDataType>
        {
            { nameof(EmployeeId), EDataType.Key },
            { nameof(EmployeeNumber), EDataType.Text },
            { nameof(EmployeeName), EDataType.Text },
            { nameof(GradeId), EDataType.Integer },
            { nameof(DesignationId), EDataType.Integer },
            { nameof(JoinDate), EDataType.Date },
            { nameof(LeaveDate), EDataType.Date },
            { nameof(IsActive), EDataType.Boolean },
            { nameof(ReplacedEmployeeId), EDataType.Integer }
        },
        nameof(EmployeeName)
    );
}
