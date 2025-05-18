using FATBuilder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NexusWPF.ViewModel;
using ProjectManager;
using SequenceSimulator;
using System.Configuration;
using System.Data;
using System.Windows;

namespace NexusWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IHost? AppHost { get; private set; }

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<NavigationVM>();

                    services.AddSingleton<IMainProjectManager, MainProjectManager>();
                    services.AddTransient<IFATManager, FATManager>();
                    services.AddTransient<ISimulator, Simulator>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost!.StartAsync();

            var startupForm = AppHost.Services.GetRequiredService<MainWindow>();
            var navigationViewModel = AppHost.Services.GetRequiredService<NavigationVM>();
            startupForm.DataContext = navigationViewModel;
            startupForm.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost!.StopAsync();
            base.OnExit(e);
        }
    }

}
