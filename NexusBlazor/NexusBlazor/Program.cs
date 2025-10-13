using DataModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using NexusBlazor.Client.Pages;
using NexusBlazor.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// 2025.09.24 Added Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "NexusAuthCookie";
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.Cookie.MaxAge = TimeSpan.FromHours(4);
        options.AccessDeniedPath = "/AccessDenied";
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// Internal Class References
builder.Services.AddScoped<Manager>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// todo: Enable HTTPS redirection once SSL is set up
// app.UseHttpsRedirection();

app.UseStaticFiles();

// 2025.09.24 Added Authentication
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(NexusBlazor.Client._Imports).Assembly);

app.Run();
