namespace FableQuestTool;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);

        var splash = new Views.SplashScreenView();
        splash.Show();

        var mainWindow = new MainWindow();
        MainWindow = mainWindow;

        mainWindow.ContentRendered += (_, _) =>
        {
            splash.Close();
        };

        mainWindow.Show();
    }
}
