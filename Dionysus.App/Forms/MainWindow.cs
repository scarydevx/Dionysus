using System.Diagnostics;
using Dionysus.App.Data;
using Dionysus.App.Helpers;
using Dionysus.App.Renders;
using Dionysus.App.Web;
using Dionysus.Web;
using Dionysus.WebScrap.GOGScrapper;
using Dionysus.WebScrap.XatabScrapper;

namespace Dionysus.App.Forms;

public partial class MainWindow : Form
{
    private bool _isExiting = false;
    private static Mutex _mutex;
    public MainWindow()
    {
        _mutex = new Mutex(false, "DionysusMutex");

        if (!_mutex.WaitOne(0, false))
        {
            MessageBox.Show("The application is already running.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit(); 
            Close();
        }

        InitializeComponent();

        this.BackColor = ColorTranslator.FromHtml("#191724");
        var _arguments = Environment.GetCommandLineArgs();
        if (_arguments.Contains("-console")) ConsoleHelper.ShowConsoleWindow();
        
        var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico");
#if  DEBUG
        this.Text = "Dionysus DEV";
#else
        this.Text = "Dionysus";
#endif
        
        this.Icon = new Icon(iconPath);
        BlazorFormsController.Activate(this.Controls);
        this.MinimumSize = new Size(1031, 733);
        this.StartPosition = FormStartPosition.CenterScreen;
        
        VisibleChanged += (sender, args) =>
        { 
            if (Visible) WindowsHelper.SetMicaTitleBar(this.Handle);
        };
        
        
        var _notifyIcon = new NotifyIcon();
        var _trayContextMenu = new ContextMenuStrip();
        _notifyIcon.Icon = new Icon(iconPath);
        _notifyIcon.Text = "Dionysus";
        _notifyIcon.ContextMenuStrip = _trayContextMenu;
        _notifyIcon.Click += (sender, args) =>
        {
            if (args is MouseEventArgs mouseArgs && mouseArgs.Button == MouseButtons.Left)
            {
                BringToFront();
                WindowState = FormWindowState.Normal;
                ShowInTaskbar = true;
                Show();   
            }
        };
        var openMenuItem = new ToolStripMenuItem("Open");
        openMenuItem.Click += (sender, args) =>
        {
            BringToFront();
            WindowState = FormWindowState.Normal;
            WindowsHelper.SetMicaTitleBar(Handle);
            ShowInTaskbar = true;
            Show();

        };
        var exitMenuItem = new ToolStripMenuItem("Exit");
        exitMenuItem.Click += (sender, args) =>
        {
            _isExiting = true; 
            Application.Exit();
        };
        _trayContextMenu.Items.Add(openMenuItem);
        _trayContextMenu.Items.Add(exitMenuItem);
        _trayContextMenu.RenderMode = ToolStripRenderMode.Professional;
        _trayContextMenu.Renderer = new ContextMenuStripRender();
        _notifyIcon.Visible = true;
        
        FormClosing += (sender, e) =>
        {
            if (!_isExiting)
            {
                e.Cancel = true;
                ShowInTaskbar = false;
                Hide();
                AppHelper.HideFromAltTab(Handle);
            }
            else
            {
                _mutex.ReleaseMutex();
                _notifyIcon.Visible = false; 
            }
        };
        
        this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        ProfileData.InitializeProfileData();
        SettingsPage.InitializeSettings();
    }
}