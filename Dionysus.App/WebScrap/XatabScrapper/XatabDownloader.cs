using System.Net;
using Dionysus.App.Logger;
using Dionysus.App.TorrentClient;
using Dionysus.Web;
using MonoTorrent;

namespace Dionysus.WebScrap.XatabScrapper;

public class XatabDownloader
{
    private static readonly HttpClient client;
    private static Logger _logger = new Logger();
    static XatabDownloader()
    {
        var handler = new HttpClientHandler
        {
            UseCookies = true,
            CookieContainer = new CookieContainer()
        };

        client = new HttpClient(handler);
    }
    
    public static async void DonwloadFile(string _siteUrl)
    {
        var _html = await client.GetStringAsync(_siteUrl);
        var _htmlDocument = new HtmlAgilityPack.HtmlDocument();
        _htmlDocument.LoadHtml(_html);

        var _torrentName = "Empty";
        var _downloadLink = "Empty";
        var _downloadLinkNode =
            _htmlDocument.DocumentNode.SelectSingleNode("//a[@class='download-torrent']");

        if (_downloadLinkNode != null)
        {
            _downloadLink = _downloadLinkNode.Attributes["href"].Value;
            _torrentName = _downloadLinkNode.Attributes["title"].Value.Replace("Скачать торрент ", "");
        }

        if (!Directory.Exists("Torrents")) Directory.CreateDirectory("Torrents");
        var _filePath = Path.Combine("Torrents", _torrentName);
        
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36");
        if (client.DefaultRequestHeaders.Contains("Referer"))
        {
            client.DefaultRequestHeaders.Remove("Referer");
        }
        client.DefaultRequestHeaders.Add("Referer", _siteUrl);
        
        var response = await client.GetAsync(_downloadLink);
        
        if (response.IsSuccessStatusCode)
        {
            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            
            await File.WriteAllBytesAsync(_filePath, fileBytes);
            _logger.Log(Logger.LogType.DEBUG, $"File '{_filePath}' downloaded.");
        }
        else
        { 
            _logger.Log(Logger.LogType.ERROR, $"Download Error: {response.StatusCode}");
        }

        if (SettingsPage.AvoidConvertToMagnet)
        {
            TorrentManager.OpenTorrentFile(_filePath);
            _logger.Log(Logger.LogType.DEBUG, $"File '{_filePath}' skipped converting to magnet.");
        }
        else
        {
            var _magnet = ConvertToMagnet(_filePath, _torrentName);
            try
            {
                TorrentManager.OpenTorrentByMagnet(_magnet);
            }
            catch (Exception e)
            {
                _logger.Log(Logger.LogType.ERROR, e.Message);
                throw;
            }
            finally
            {
                File.Delete(_filePath);
                _logger.Log(Logger.LogType.DEBUG, $"File '{_filePath}' deleted.");
            }   
        }
    }

    static string ConvertToMagnet(string _filePath, string _fileName)
    {
        try
        {
            var _torrent = Torrent.Load(_filePath);
            var _hash = _torrent.InfoHashes.V1.ToHex();
            _logger.Log(Logger.LogType.DEBUG, $"Torrent '{_filePath}' converted to magnet link");
            return $"magnet:?xt=urn:btih:{_hash}&dn={_fileName}";
        }
        catch (Exception e)
        {
            _logger.Log(Logger.LogType.ERROR, e.Message);
            throw new Exception("Torrent corrupted.");
        }

        return "emptyHash";
    }
}