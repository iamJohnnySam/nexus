using NexusWPF.Utilities;
using ProjectManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusWPF.Commands.ReviewCommands
{
    class ShowAllReviewItemsCommand : CommandBase
    {
        private readonly ReviewManager reviewManager;

        public override void Execute(object? parameter)
        {
            reviewManager.UpdateAllReviewData();
        }

        public ShowAllReviewItemsCommand(ReviewManager reviewManager)
        {
            this.reviewManager = reviewManager;
        }

    }
}
