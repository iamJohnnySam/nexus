using NexusWPF.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Input;

namespace NexusWPF.ViewModel
{
    public class NavigationVM : ViewModelBase
    {
        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }

        public ICommand HomeCommand { get; set; }
        public ICommand ProjectsCommand { get; set; }

        private void Home(object obj) => CurrentView = new HomeVM();
        private void Projects(object obj) => CurrentView = new ProjectsVM();

        public NavigationVM()
        {
            HomeCommand = new RelayCommand(Home);
            ProjectsCommand = new RelayCommand(Projects);

            // Startup Page
            CurrentView = new HomeVM();

        }
    }
}
