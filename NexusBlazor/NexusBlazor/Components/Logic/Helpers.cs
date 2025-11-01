using DataModels;
using DataModels.Administration;
using DataModels.Data;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace NexusBlazor.Components.Logic;

public static class Helpers
{
    public static async Task<Employee?> GetCurrentEmployee(Manager manager, LoginInformation LoginInfo, AuthenticationStateProvider AuthStateProvider)
    {
        if (LoginInfo.CurrentEmployee == null)
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                int id = int.Parse(user.FindFirst(ClaimTypes.SerialNumber)?.Value);
                LoginInfo.CurrentEmployeeId = id;
                LoginInfo.CurrentEmployee = await manager.EmployeeDB.GetByIdAsync(id);
            }
            else
            {
                return null;
            }
        }
        return LoginInfo.CurrentEmployee;
    }
}
