using Dionysus.App.Forms;

namespace Dionysus.App;

static class Program
{
    public static readonly string _appCodeName = "Lotus";
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new MainWindow());
    }
}
