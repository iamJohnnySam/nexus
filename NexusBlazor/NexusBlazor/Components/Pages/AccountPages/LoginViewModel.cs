using System.ComponentModel.DataAnnotations;

namespace NexusBlazor.Components.Pages.AccountPages;

public class LoginViewModel
{
    public string EmployeeId { get; set; } = "39";

    [Required(AllowEmptyStrings = false, ErrorMessage = "Password is required")]
    public string? Password { get; set; }
}
