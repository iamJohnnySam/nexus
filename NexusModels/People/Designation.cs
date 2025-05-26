using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusModels.People;

public class Designation
{
    [Key]
    public int DesignationId { get; set; }
    public required string DesignationName { get; set; }
    public string? Department {  get; set; }
}
