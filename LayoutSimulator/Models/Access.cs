using LayoutSimulator.Enums;
using LayoutSimulator.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LayoutSimulator.Models;

public class Access : INotifyPropertyChanged
{
    public bool HasDoor { get; internal set; }
	private EDoorStatus doorStatus;

	public EDoorStatus DoorStatus
	{
		get { return doorStatus; }
		set 
        { 
            doorStatus = value;
            OnPropertyChanged();
        }
	}

    public bool IsAccessible { get
        {
            if(!HasDoor)
                return true;
            else
            {
                if (DoorStatus == EDoorStatus.Open)
                    return true;
                else
                    return false;
            }
        }
    }

    public uint DoorTransitionTime { get; set; }

    public Access(bool hasDoor, uint transitionTime)
    {
        HasDoor = hasDoor;
        if (HasDoor)
            DoorStatus = EDoorStatus.Closed;
        else
            DoorStatus = EDoorStatus.Open;
        DoorTransitionTime = transitionTime;
    }

    public void OpenDoor()
    {
        if (HasDoor)
        {
            DoorStatus = EDoorStatus.Opening;
            InternalClock.Instance.ProcessWait(DoorTransitionTime);
            DoorStatus = EDoorStatus.Open;
        }
        else
        {
            DoorStatus = EDoorStatus.Open;
        }
    }

    public void CloseDoor()
    {
        if (HasDoor)
        {
            DoorStatus = EDoorStatus.Closing;
            InternalClock.Instance.ProcessWait(DoorTransitionTime);
            DoorStatus = EDoorStatus.Closed;
        }
        else
        {
            DoorStatus = EDoorStatus.Open;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
