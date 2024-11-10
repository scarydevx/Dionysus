using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;

namespace Dionysus.App.Web;

public class BlazorFormsController
{
    public static BlazorWebView _webView = new BlazorWebView();
    public static void Activate(Control.ControlCollection controls)
    {
        _webView.Dock = DockStyle.Fill;
        
        var _services = new ServiceCollection();
        _services.AddWindowsFormsBlazorWebView();
        _webView.HostPage = "Web\\wwwroot\\index.html";
        _webView.Services = _services.BuildServiceProvider();
        _webView.RootComponents.Add<Dionysus.Web.App>("#app");
        _webView.RootComponents.Add<HeadOutlet>("head::after");
        _webView.BackColor = Color.Black;
        
        controls.Add(_webView);
    }
}