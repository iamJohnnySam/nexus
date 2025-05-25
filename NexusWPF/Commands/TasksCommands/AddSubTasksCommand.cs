using NexusModels;
using NexusWPF.Utilities;
using NexusWPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NexusWPF.Commands.TasksCommands
{
    public class AddSubTasksCommand : CommandBase
    {
        private readonly TasksVM tasksVM;

        public override void Execute(object? parameter)
        {
            if (parameter is not null and TaskItem)
            {
                tasksVM.AddTask(parameter as TaskItem);
            }
        }

        public AddSubTasksCommand(TasksVM tasksVM)
        {
            this.tasksVM = tasksVM;
        }
    }
}
