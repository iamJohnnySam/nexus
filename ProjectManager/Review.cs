using NexusModels;
using NexusModels.Enums;
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
        public Module ModuleUnderReview => ReviewPoint.ModuleUnderTest;
        public bool Completed
        {
            get
            {
                if (ProjectFeedback == null)
                    return false;
                return ProjectFeedback.Approved;
            }
            set
            {
            }
        }
    }
}
