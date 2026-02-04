namespace FableQuestTool;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);

        var splash = new Views.SplashScreenView();
        var splashStart = System.DateTime.UtcNow;
        splash.Show();

        var mainWindow = new MainWindow();
        MainWindow = mainWindow;

        mainWindow.ContentRendered += async (_, _) =>
        {
            var elapsed = System.DateTime.UtcNow - splashStart;
            var remaining = System.TimeSpan.FromSeconds(3) - elapsed;
            if (remaining > System.TimeSpan.Zero)
            {
                await System.Threading.Tasks.Task.Delay(remaining);
            }

            splash.Close();
        };

        mainWindow.Show();
    }
}
