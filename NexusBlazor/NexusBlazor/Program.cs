using DataModels;
using DataModels.Administration;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using NexusBlazor.Components;
using NexusBlazor.Components.Logic;
using NexusMaintenance;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// 2025.09.24 Added Authorization
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "NexusAuthCookie";
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.Cookie.MaxAge = TimeSpan.FromHours(4);
        options.AccessDeniedPath = "/access_denied";
    });
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

// 2025.10.14 to get session information
builder.Services.AddHttpContextAccessor();

// Internal Class References
builder.Services.AddSingleton<Manager>();
builder.Services.AddSingleton<SqliteLogger>();
builder.Services.AddScoped<LoginInformation>();

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
