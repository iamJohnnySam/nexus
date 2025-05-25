using NexusWPF.Utilities;
using ProjectManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusWPF.Commands.ReviewCommands
{
    class DeleteReviewPointCommand : CommandBase
    {
        private readonly ReviewManager reviewManager;

        public override void Execute(object? parameter)
        {
            reviewManager.DeleteReviewPoint();
        }

        public DeleteReviewPointCommand(ReviewManager reviewManager)
        {
            this.reviewManager = reviewManager;
        }

    }
}
