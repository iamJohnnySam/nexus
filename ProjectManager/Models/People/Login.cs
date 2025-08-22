using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager.Models;

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
}
