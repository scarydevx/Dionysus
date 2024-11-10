using System.Runtime.InteropServices;

namespace Dionysus.App.Helpers;

public class ConsoleHelper
{
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool AllocConsole();

    public static void ShowConsoleWindow() => AllocConsole();
}