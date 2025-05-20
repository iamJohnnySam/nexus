using NexusModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager
{
    public class Review
    {
        public required ReviewPoint ReviewPoint { get; set; }
        public ProjectReviewItem? ProjectFeedback { get; set; }
    }
}
