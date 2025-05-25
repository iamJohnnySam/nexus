using NexusWPF.Utilities;
using NexusWPF.ViewModel;
using ProjectManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusWPF.Commands.ReviewCommands
{
    class CreateReviewPointCommand : CommandBase
    {
        private readonly ReviewVM reviewVM;
        private readonly ReviewManager reviewManager;

        public override void Execute(object? parameter)
        {
            reviewManager.CreateReviewPoint(reviewVM.ModuleUnderTest, reviewVM.NewReviewPoint);
            reviewVM.NewReviewPoint = string.Empty;
        }

        public CreateReviewPointCommand(ReviewVM reviewVM, ReviewManager reviewManager)
        {
            this.reviewVM = reviewVM;
            this.reviewManager = reviewManager;
        }
    }
}
