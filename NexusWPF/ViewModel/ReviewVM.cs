using NexusModels.Enums;
using NexusWPF.Commands.ReviewCommands;
using ProjectManager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UIUtilities;

namespace NexusWPF.ViewModel
{
    class ReviewVM : ViewModelBase
    {
        private readonly IMainProjectManager projectManager;
        private ReviewManager reviewManager;

        // All Reviews
        public ObservableCollection<Review> Reviews => reviewManager.Reviews;

        // Add new Review Point
        private string _newReviewPoint = string.Empty;
        public string NewReviewPoint
        {
            get
            {
                return _newReviewPoint;
            }
            set
            {
                _newReviewPoint = value;
                OnPropertyChanged();
            }
        }

        // Module dropdown
        private Module _module;
        public Module ModuleUnderTest
        {
            get
            {
                return _module;
            }
            set
            {
                _module = value;
                reviewManager.UpdateReviewData([value]);
                OnPropertyChanged();
            }
        }


        // Selected Review
        public Review? CurrentReview
        {
            get
            {
                return reviewManager.CurrentReview;
            }
            set
            {
                reviewManager.CurrentReview = value;
                OnPropertyChanged();
            }
        }
        public bool IsReviewSelected {
            get
            {
                return CurrentReview != null;
            }
        }
        public string CurrentReviewTitle
        {
            get 
            {
                if (CurrentReview != null)
                    return CurrentReview.ReviewPoint.ReviewItem;
                return "No Review Selected";
            }
        }
        public string CurrentReviewModule
        {
            get
            {
                if (CurrentReview != null)
                    return CurrentReview.ReviewPoint.ModuleUnderTest.ToString();
                return "Select a review or create a review to get started.";
            }
        }
        public string CurrentReviewComments
        {
            get
            {
                if (CurrentReview == null)
                    return string.Empty;
                return CurrentReview.ProjectFeedback == null ? string.Empty : CurrentReview.ProjectFeedback.ReviewComments;
            }
            set
            {
                reviewManager.CreateReviewFeedback();
                if (CurrentReview.ProjectFeedback != null)
                    CurrentReview.ProjectFeedback.ReviewComments = value;
                OnPropertyChanged();
            }
        }
        public bool CurrentReviewApproval
        {
            get
            {
                if (CurrentReview == null)
                    return false;
                return CurrentReview.ProjectFeedback == null ? false : CurrentReview.ProjectFeedback.Approved;
            }
            set
            {
                reviewManager.CreateReviewFeedback();
                if(CurrentReview.ProjectFeedback != null)
                    CurrentReview.ProjectFeedback.Approved = value;
                OnPropertyChanged();
            }
        }


        public ICommand CreateReviewPoint { get; }
        public ICommand DeleteReviewPoint { get; }
        public ICommand ShowAllReviewPoints { get; }
        public ICommand ApproveReview { get; }
        public ICommand RejectReview { get; }
        public ICommand SaveReview { get; }

        public ReviewVM(IMainProjectManager projectManager)
        {
            this.projectManager = projectManager;
            reviewManager = new(projectManager.CurrentProject!);

            CreateReviewPoint = new CreateReviewPointCommand(this, reviewManager);
            DeleteReviewPoint = new DeleteReviewPointCommand(reviewManager);
            ShowAllReviewPoints = new ShowAllReviewItemsCommand(reviewManager);
            ApproveReview = new ApproveReviewCommand(reviewManager);
            RejectReview = new RejectReviewCommand(reviewManager);
            SaveReview = new SaveReviewCommand(reviewManager);



            projectManager.PropertyChanged += (s, e) =>
            {

            };

            reviewManager.PropertyChanged += (s, e) =>
            {
                if(e.PropertyName == (nameof(reviewManager.CurrentReview)))
                {
                    OnPropertyChanged(nameof(CurrentReview));
                    OnPropertyChanged(nameof(CurrentReviewTitle));
                    OnPropertyChanged(nameof(CurrentReviewModule));
                    OnPropertyChanged(nameof(CurrentReviewComments));
                    OnPropertyChanged(nameof(CurrentReviewApproval));
                    OnPropertyChanged(nameof(IsReviewSelected));
                }
                if(e.PropertyName == (nameof(reviewManager.Reviews)))
                {
                    OnPropertyChanged(nameof(Reviews));
                }
            };
        }


    }
}
