using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Dionysus.App.Helpers;

public class WindowsHelper
{
    [DllImport("dwmapi.dll", PreserveSig = true)]
    public static extern int DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE attribute, ref int pvAttribute, int cbAttribute);
    
    public enum DWMWINDOWATTRIBUTE
    {
        DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
        DWMWA_MICA_EFFECT = 1029
    }
    
    static bool IsWindows11()
    {
        var _reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

        var _currentBuildStr = (string)_reg.GetValue("CurrentBuild");
        var _currentBuild = int.Parse(_currentBuildStr);

        return _currentBuild >= 22000;
    }

    public static void SetMicaTitleBar(IntPtr handle)
    {
        if (IsWindows11())
        {
            var preference = 1; 
            DwmSetWindowAttribute(handle,
                DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, 
                ref preference, sizeof(int));
        }
    }
}