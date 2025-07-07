using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager.Models;

public class Deliverables
{
    [Key]
    public int DeliverableId { get; set; }
    public string DeliverableName { get; set; }= string.Empty;
    public string DeliverableDescription { get; set; }= string.Empty;
    public string DeliverableType {  get; set; }= string.Empty;

}
