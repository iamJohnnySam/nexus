using DataModels;
using DataModels.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using NexusBlazor.Components.Pages.DepartmentPages;
using NexusMaintenance;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NexusBlazor.Components.Logic;

public class LoginInformation : INotifyPropertyChanged
{
    AuthenticationStateProvider AuthStateProvider;
    Manager manager;
    public ClaimsPrincipal? User { get; set; } // Fix for CS0246: Replace 'User' with 'ClaimsPrincipal' (from System.Security.Claims)
                                            // Fix for IDE1006: Rename property to 'User' (PascalCase)

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

	private Employee? currentEmployee;
	public Employee? CurrentEmployee
	{
		get 
		{
            if (currentEmployee == null)
            {
                var authState = AuthStateProvider.GetAuthenticationStateAsync().Result;
                var user = authState.User;

                if (user.Identity?.IsAuthenticated == true)
                {
                    int id = int.Parse(user.FindFirst(ClaimTypes.SerialNumber)?.Value);
                    CurrentEmployeeId = id;
                    currentEmployee = manager.EmployeeDB.GetByIdAsync(id).Result;
                }
                else
                {
                    return null;
                }
            }
            return currentEmployee; }
		set { currentEmployee = value;
            OnPropertyChanged();
        }
	}

	public bool LoggedIn { get; set; } = false;

    public event PropertyChangedEventHandler? PropertyChanged;

    public LoginInformation(AuthenticationStateProvider authStateProvider, IHttpContextAccessor httpContextAccessor, Manager manager, SqliteLogger logger)
    {
        AuthStateProvider = authStateProvider;
        this.manager = manager;

        var sessionId = Guid.NewGuid().ToString();
        logger.Info($"New session started: {sessionId}");
        User = httpContextAccessor.HttpContext?.User;
        logger.SetSessionContext(sessionId, User?.Identity?.Name ?? "Anonymous");
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
