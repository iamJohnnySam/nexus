using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class TaskItemProjectLink:INotifyPropertyChanged
{
    int projectId;

    private List<TaskItem> incompleteParentTasks = [];
    public List<TaskItem> IncompleteParentTasks
    {
        get
        {
            return incompleteParentTasks;
        }
        set
        {
            incompleteParentTasks = value;
            OnPropertyChanged();
        }
    }

    private List<TaskItem> completedParentTasks = [];
    public List<TaskItem> CompletedParentTasks
    {
        get
        {
            return completedParentTasks;
        }
        set
        {
            completedParentTasks = value;
            OnPropertyChanged();
        }
    }

    private Dictionary<int, List<TaskItem>> subTasks = [];
    public Dictionary<int, List<TaskItem>> SubTasks
    {
        get
        {
            return subTasks;
        }
        set
        {
            subTasks = value;
            OnPropertyChanged();
        }
    }

    public TaskItemProjectLink(int projectId)
    {
        this.projectId = projectId;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
