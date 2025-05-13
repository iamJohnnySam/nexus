using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NexusWPF.View.SimulationView;

namespace NexusWPF.Services
{
    public class SimulatorWindowService : IWindowService
    {

        public void OpenWindow(object dataContext)
        {
            SequenceSimulationWindow window = new();
            window.DataContext = dataContext;
            window.Show();
        }


        public void CloseWindow()
        {
            // Get a reference to the current window
            var window = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

            // Close the window
            if (window != null)
            {
                window.Close();
            }
        }


    }
}
