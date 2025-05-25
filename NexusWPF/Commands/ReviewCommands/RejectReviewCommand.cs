using NexusWPF.Utilities;
using ProjectManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusWPF.Commands.ReviewCommands
{
    class RejectReviewCommand : CommandBase
    {
        private readonly ReviewManager reviewManager;

        public override void Execute(object? parameter)
        {
            reviewManager.UpdateApprovalStatus(false);
        }

        public RejectReviewCommand(ReviewManager reviewManager)
        {
            this.reviewManager = reviewManager;
        }

    }
}
