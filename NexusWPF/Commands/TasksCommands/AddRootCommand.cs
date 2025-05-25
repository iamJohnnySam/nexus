using NexusWPF.Utilities;
using NexusWPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusWPF.Commands.TasksCommands
{
    public class AddRootCommand : CommandBase
    {
        private readonly TasksVM tasksVM;

        public override void Execute(object? parameter)
        {
            tasksVM.AddTask();
        }

        public AddRootCommand(TasksVM tasksVM)
        {
            this.tasksVM = tasksVM;
        }
    }
}
