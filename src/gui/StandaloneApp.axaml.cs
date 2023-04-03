using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace SearchAThing.OpenGL.GUI;

/// <summary>
/// Avalonia application for stand-alone console program.<br/>
/// This will run in a thread (ui) until <see cref="GLWindow.ShowSync"/> completed.
/// </summary>
internal class StandaloneApp : Application
{
    AutoResetEvent? are = null;

    public StandaloneApp()
    {
    }

    public StandaloneApp(AutoResetEvent are)
    {
        this.are = are;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
  
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            //ExpressionObserver.DataValidators.RemoveAll(x => x is DataAnnotationsValidationPlugin);
            // desktop.MainWindow = new MainWindow
            // {
            //     DataContext = new MainWindowViewModel(),
            // };
        }

        base.OnFrameworkInitializationCompleted();

        are?.Set();
    }


}

public static partial class Toolkit
{

    static bool sa_initialized = false;

    /// <summary>
    /// Initialize avalonia for stand-alone application.<br/>
    /// This must be called before any other avalonia control usages.
    /// </summary>
    public static void InitAvalonia()
    {
        if (sa_initialized) return;

        sa_initialized = true;

        var are = new AutoResetEvent(false);

        Task.Run(() =>
        {
            var appBuilder = AppBuilder
                .Configure(() =>
                {
                    var app = new StandaloneApp(are);

                    return app;
                })
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();

            appBuilder.StartWithClassicDesktopLifetime(new string[] { }, ShutdownMode.OnExplicitShutdown);
        });

        are.WaitOne();
    }

}
