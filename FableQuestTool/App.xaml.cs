namespace FableQuestTool;

/// <summary>
/// Application entry point that handles startup flow and splash timing.
/// </summary>
public partial class App : System.Windows.Application
{
    /// <summary>
    /// Boots the main window and optionally shows the splash screen.
    /// </summary>
    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);

        Views.SplashScreenView? splash = null;
        System.DateTime? splashStart = null;
        var config = Config.FableConfig.Load();
        if (config.GetShowStartupImage())
        {
            splash = new Views.SplashScreenView();
            splashStart = System.DateTime.UtcNow;
            splash.Show();
        }

        var mainWindow = new MainWindow();
        MainWindow = mainWindow;

        mainWindow.ContentRendered += async (_, _) =>
        {
            if (splash != null && splashStart.HasValue)
            {
                var elapsed = System.DateTime.UtcNow - splashStart.Value;
                var remaining = System.TimeSpan.FromSeconds(3) - elapsed;
                if (remaining > System.TimeSpan.Zero)
                {
                    await System.Threading.Tasks.Task.Delay(remaining);
                }

                splash.Close();
            }
        };

        mainWindow.Show();
    }
}
