using NexusModels.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusModels
{
    public class ReviewPoint
    {
        [Key]
        public int ReviewId { get; set; }
        public Module ModuleUnderTest { get; set; }
        public required string ReviewItem { get; set; }
    }
}
