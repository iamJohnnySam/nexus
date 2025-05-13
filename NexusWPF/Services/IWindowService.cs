using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusWPF.Services
{
    public interface IWindowService
    {
        void OpenWindow(object dataContext);
        void CloseWindow();
    }
}
