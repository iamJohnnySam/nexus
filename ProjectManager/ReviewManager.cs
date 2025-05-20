using NexusModels;
using ProjectManager.DB;
using ProjectManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager
{
    public class ReviewManager
    {
        private readonly Project project;
        public ObservableCollection<Review> Reviews { get; set; } = [];


        public ReviewManager(Project project)
        {
            this.project = project;
        }

        public void UpdateReviewData()
        {
            Reviews = [];
            using var context = new AppDbContext();

            List<ReviewPoint> ReviewItems = context.Reviews.ToList();
            foreach (ReviewPoint item in ReviewItems)
            {
                ProjectReviewItem reviewFeedback = context.ProjectReviews.SingleOrDefault(x => x.ProjectId == project.ProjectId && x.ReviewPoint.ReviewId == item.ReviewId);

                Reviews.Add(new Review
                {
                    ReviewPoint = item,
                    ProjectFeedback = reviewFeedback
                });
            }

        }
    }
}
