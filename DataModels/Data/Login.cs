using DataModels.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class Login
{
    [Key]
    public int LoginId { get; set; }
    public int EmployeeId { get; set; } = 0;
    public string Password { get; set; } = string.Empty;
    public DateTime LoginCreated { get; set; } = DateTime.Now;
    public DateTime LastLogin { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;
    public bool Administrator { get; set; } = false;

    public static TableMetadata Metadata => new(
        typeof(Login).Name,
        new Dictionary<string, EDataType>
        {
            { nameof(LoginId), EDataType.Key },
            { nameof(EmployeeId), EDataType.Integer },
            { nameof(Password), EDataType.Text },
            { nameof(LoginCreated), EDataType.Text },
            { nameof(LastLogin), EDataType.Text },
            { nameof(IsActive), EDataType.Integer },
            { nameof(Administrator), EDataType.Integer }
        },
        nameof(EmployeeId)
    );
}
