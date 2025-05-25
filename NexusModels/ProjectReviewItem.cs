using ProjectManager.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusModels
{
    public class ProjectReviewItem
    {
        [Key]
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public required Project Project { get; set; }
        public required ReviewPoint ReviewPoint { get; set; }
        public bool Approved { get; set; } = false;
        public DateTime? LastReviewDate { get; set; }
        public string ReviewComments { get; set; } = string.Empty;

    }
}
