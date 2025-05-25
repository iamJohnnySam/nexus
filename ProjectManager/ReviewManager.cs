using NexusModels;
using NexusModels.Enums;
using ProjectManager.DB;
using ProjectManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIUtilities;

namespace ProjectManager
{
    public class ReviewManager : ViewModelBase
    {
        private readonly Project project;
        public ObservableCollection<Review> Reviews { get; set; } = [];
        private Review? currentReview;
        public Review? CurrentReview
        {
            get
            {
                return currentReview;
            }
            set
            {
                currentReview = value;
                OnPropertyChanged();
            }
        }


        public ReviewManager(Project project)
        {
            this.project = project;
            UpdateAllReviewData();
        }

        public void UpdateAllReviewData()
        {
            Reviews = [];
            using var context = new AppDbContext();

            var moduleEnums = project.Modules;

            List<ReviewPoint> ReviewItems = context.Reviews
                .Where(r => moduleEnums.Contains(r.ModuleUnderTest))
                .ToList();

            foreach (ReviewPoint item in ReviewItems)
            {
                CreateReview(item);
            }
            OnPropertyChanged(nameof(Reviews));
        }

        public void UpdateReviewData(List<Module> modules)
        {
            Reviews = [];
            using var context = new AppDbContext();

            var moduleEnums = modules;

            List<ReviewPoint> ReviewItems = context.Reviews
                .Where(r => moduleEnums.Contains(r.ModuleUnderTest))
                .ToList();

            foreach (ReviewPoint item in ReviewItems)
            {
                CreateReview(item);
            }
            OnPropertyChanged(nameof(Reviews));
        }

        public void CreateReview(ReviewPoint point)
        {
            using var context = new AppDbContext();
            ProjectReviewItem reviewFeedback = context.ProjectReviews.SingleOrDefault(x => x.ProjectId == project.ProjectId && x.ReviewPoint.ReviewId == point.ReviewId);

            Reviews.Add( new Review
            {
                ReviewPoint = point,
                ProjectFeedback = reviewFeedback
            });
        }
        public void CreateReviewPoint(Module moduleUnderTest, string reviewItem)
        {
            ReviewPoint reviewPoint = new ReviewPoint()
            {
                ModuleUnderTest = moduleUnderTest,
                ReviewItem = reviewItem
            };

            using var context = new AppDbContext();
            context.Reviews.Add(reviewPoint);
            context.SaveChanges();

            CreateReview(reviewPoint);
            OnPropertyChanged(nameof(Reviews));

        }
        public void DeleteReviewPoint()
        {
            if(CurrentReview != null)
            {
                using var context = new AppDbContext();
                context.Reviews.Remove(CurrentReview.ReviewPoint);
                context.SaveChanges();
                Reviews.Remove(CurrentReview);
                CurrentReview = null;
            }
        }

        public void CreateReviewFeedback()
        {
            if (CurrentReview != null && CurrentReview.ProjectFeedback == null)
            {
                CurrentReview.ProjectFeedback = new()
                {
                    Project = project,
                    ReviewPoint = CurrentReview.ReviewPoint
                };
            }
        }
        public void UpdateApprovalStatus(bool status)
        {
            if(CurrentReview != null && CurrentReview.ProjectFeedback != null)
            {
                CurrentReview.ProjectFeedback.Approved = status;
                SaveReview();
            }
        }

        public void SaveReview()
        {
            if(CurrentReview != null && CurrentReview.ProjectFeedback != null)
            {
                using var context = new AppDbContext();
                context.ProjectReviews.Update(currentReview.ProjectFeedback);
                context.SaveChanges();
            }
        }

    }
}
