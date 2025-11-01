using DataModels.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Administration;

public class LoginInformation : INotifyPropertyChanged
{
	private Project? currentProject;

	public Project CurrentProject
	{
		get
		{ 
			if (currentProject == null)
			{
				currentProject = new ManagerLite().ProjectDB.GetByIdAsync(1).Result!;
            }
            return currentProject;
		}
		set 
		{ 
			if(value != currentProject) 
			{
				currentProject = value; 
				OnPropertyChanged(); 
			}
        }
	}

    public int CurrentEmployeeId { get; set; } = 0;
    public Employee? CurrentEmployee { get; set; }
    public bool LoggedIn { get; set; } = false;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
