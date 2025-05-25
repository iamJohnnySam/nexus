using Microsoft.EntityFrameworkCore;
using NexusModels;
using NexusWPF.Commands.TasksCommands;
using NexusWPF.View;
using ProjectManager;
using ProjectManager.DB;
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
    public class TasksVM : ViewModelBase
    {
        private readonly IMainProjectManager projectManager;

        public ObservableCollection<TaskItem> Tasks { get; set; } = new();

        private readonly AppDbContext _dbContext = new();

        public ICommand AddRootCommand { get; }
        public ICommand AddSubTaskCommand { get; }

        public async void AddTask(TaskItem? parent = null)
        {
            if (projectManager.CurrentProject != null)
            {
                TaskItem newTask = new()
                {
                    Title = "New Task",
                    Project = projectManager.CurrentProject,
                    Deadline = DateTime.Now
                };

                if (parent == null)
                    Tasks.Add(newTask);
                else
                {
                    parent.SubTasks.Add(newTask);
                    newTask.ParentTaskItemId = parent.TaskId;
                }
                await SaveTaskAsync(newTask);
            }
        }

        public void SortTasks()
        {
            Tasks = new ObservableCollection<TaskItem>(Tasks.OrderBy(t => t.IsFullyCompleted).ThenBy(t => t.Deadline));
            OnPropertyChanged(nameof(Tasks));
        }

        public async Task LoadTasksAsync()
        {
            var tasks = await _dbContext.TaskItems
                .Where(t => t.ParentTaskItemId == null)
                .Include(t => t.SubTasks)
                .ToListAsync();

            Tasks = new ObservableCollection<TaskItem>(tasks);
            OnPropertyChanged(nameof(View.Tasks));
        }

        public async Task SaveTaskAsync(TaskItem task)
        {
            _dbContext.TaskItems.Add(task);
            await _dbContext.SaveChangesAsync();
        }

        public TasksVM(IMainProjectManager projectManager)
        {
            this.projectManager = projectManager;
            AddRootCommand = new AddRootCommand(this);
            AddSubTaskCommand = new AddSubTasksCommand(this);

            LoadTasksAsync().Wait();
        }
    }
}
