using ProjectManager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIUtilities;

namespace NexusWPF.ViewModel
{
    class ReviewVM : ViewModelBase
    {
        private readonly IMainProjectManager projectManager;
        private ReviewManager reviewManager;

        public ObservableCollection<Review> Reviews => reviewManager.Reviews;

        public ReviewVM(IMainProjectManager projectManager)
        {
            this.projectManager = projectManager;
            reviewManager = new(projectManager.CurrentProject!);
        }


    }
}
