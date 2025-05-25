using ProjectManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NexusModels
{
    public class TaskItem : INotifyPropertyChanged
    {
        private string _title;
        private string _responsible;
        private DateTime _deadline;
        private bool _isCompleted;

        [Key]
        public int TaskId { get; set; }
        public int ProjectId { get; set; }
        public required Project Project { get; set; }

        public required string Title 
        { 
            get => _title;
            set 
            { 
                _title = value; OnPropertyChanged(); 
            } 
        }
        public string Responsible 
        { 
            get => _responsible; 
            set 
            { 
                _responsible = value; OnPropertyChanged(); 
            } 
        }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime Deadline 
        { 
            get => _deadline; 
            set 
            {
                _deadline = value; OnPropertyChanged(); 
            } 
        }
        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                _isCompleted = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsFullyCompleted));
            }
        }

        public int? ParentTaskItemId { get; set; }
        public TaskItem? ParentTaskItem { get; set; }

        public ObservableCollection<TaskItem> SubTasks { get; set; } = new();

        public bool IsFullyCompleted => IsCompleted && SubTasks.All(s => s.IsFullyCompleted);

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
