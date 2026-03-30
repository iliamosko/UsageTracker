using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using UsageTracker.Data;
using UsageTracker.Services;
using UsageTracker.ViewModels;
using UsageTracker.Views;

namespace UsageTracker;

public partial class App : Application
{
    private static IServiceProvider? _services;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var loginVm = _services.GetRequiredService<LoginViewModel>();
            desktop.MainWindow = new LoginView { DataContext = loginVm };
        }

        base.OnFrameworkInitializationCompleted();
    }

    public static T GetService<T>() where T : notnull =>
        _services!.GetRequiredService<T>();

    private static void ConfigureServices(IServiceCollection services)
    {
        // Services
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<IWindowMonitor, WindowMonitor>();
        services.AddTransient<IProcessTrackingService, ProcessTrackingService>();

        // ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<TrackingViewModel>();
    }
}
