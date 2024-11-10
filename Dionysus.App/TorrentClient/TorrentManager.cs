using System.Diagnostics;
using Dionysus.Web;
using Dionysus.WebScrap.XatabScrapper;

namespace Dionysus.App.TorrentClient;

public class TorrentManager
{
    public static void OpenTorrentByMagnet(string _magnet)
    {
        var _startInfo = new ProcessStartInfo
        {
            FileName = _magnet,
            UseShellExecute = true
        };

        Process.Start(_startInfo);
    }
    
    public static void DownloadTypeSelector(string _gameLink, string _downloadLink)
    {
        if (_downloadLink.StartsWith("magnet:")) OpenTorrentByMagnet(_downloadLink);
        else TorrentManager.DownloadTorrentFile(_gameLink);
        Toast.toast.ShowToast("Downloading started");
    } 
    
    public static async void DownloadTorrentFile(string url)
    {
        XatabDownloader.DonwloadFile(url);
    }
    
    public static void OpenTorrentFile(string path)
    {
        using Process fileopener = new Process();

        fileopener.StartInfo.FileName = "C:\\Program Files\\qBittorrent\\qbittorrent.exe";
        fileopener.StartInfo.Arguments = $"\"{path}\"";
        fileopener.StartInfo.UseShellExecute = true;
        fileopener.Start();
    }
    
    
}