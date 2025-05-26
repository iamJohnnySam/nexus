using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusModels.People;

public class Grade
{
    [Key]
    public int GradeId { get; set; }
    public required string GradeName { get; set; }
}
