using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class TaskItemParentTaskLink : INotifyPropertyChanged
{
    int parentTaskId;

	private List<TaskItem> subTasks = [];
	public List<TaskItem> SubTasks
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


	public TaskItemParentTaskLink(int parentTaskId)
    {
        this.parentTaskId = parentTaskId;
    }


    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
