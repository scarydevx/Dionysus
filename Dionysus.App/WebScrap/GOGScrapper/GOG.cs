using System.Text.RegularExpressions;
using craftersmine.SteamGridDBNet;
using Dionysus.App.Data;
using Dionysus.App.Logger;
using Dionysus.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dionysus.WebScrap.GOGScrapper;

public class GOG
{
    private static Logger _logger = new Logger();

    public static async Task<bool> GetStatus()
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync("https://freegogpcgames.com");
                if (response.IsSuccessStatusCode)
                {
                    _logger.Log(Logger.LogType.DEBUG,
                        $"Website https://freegogpcgames.com is available. Status: {response.StatusCode}");
                    return true;
                }
                else
                {
                    _logger.Log(Logger.LogType.DEBUG,
                        $"Website https://freegogpcgames.com is unavailable. Status: {response.StatusCode}");
                    Console.WriteLine();
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Log(Logger.LogType.ERROR, ex.Message);
            return false;
        }
    }

    public static async Task<IEnumerable<SearchGameInfoStruct>> GetSearchResponse(string _request)
    {
        var _siteLink = $"https://freegogpcgames.com/?s={_request}";
        var _responseList = new List<SearchGameInfoStruct>();
        try
        {
            using var _httpClient = new HttpClient();
            var _html = await _httpClient.GetStringAsync(_siteLink);
            var _htmlDocument = new HtmlAgilityPack.HtmlDocument();
            _htmlDocument.LoadHtml(_html);

            var _responseDivs =
                _htmlDocument.DocumentNode.SelectNodes("//div[contains(@class, 'generate-columns-container')]/article");
            if (_responseDivs != null)
            {
                var tasks = _responseDivs.Select(async _div =>
                {
                    var _name = _div.SelectSingleNode(".//header/h2/a");
                    if (_name != null)
                    {
                        var _link = _name.Attributes["href"]?.Value;
                        if (_link != null)
                        {
                            var _rephrasedName = _name.InnerText.Trim()
                                .Replace("&#8211;", "-")
                                .Replace("&#038;", "&")
                                .Replace("&#8217;", "`")
                                .Replace(":", "")
                                .Replace("-", "");
                            var _rephrasedRequest = _request.Replace(":", "").Replace("-", "");

                            if (_rephrasedName.Contains(_rephrasedRequest))
                            {
                                var (downloadLink, size, date, version) = await GetDataFromLink(_link);
                                if (!string.IsNullOrEmpty(size))
                                {
                                    size = size.StartsWith("Note:")
                                        ? null
                                        : size.Replace("Size: ", "").Replace("GiB", "GB");
                                }
                                else { size = string.Empty; }
                                
                                var _downloadLink = BypassDownloadLink(downloadLink);
                                _responseList.Add(new SearchGameInfoStruct()
                                {
                                    Name = _rephrasedName,
                                    Link = _link,
                                    Date = !string.IsNullOrEmpty(date) ? DateTime.Parse(date).ToString("dd/MM/yyyy").Replace("/",".") : string.Empty,
                                    Version = version.Replace("v",""),
                                    Size = size,
                                    DownloadLink = _downloadLink
                                });
                            }
                        }
                    }
                });

                await Task.WhenAll(tasks);
            }
        }
        catch (Exception e)
        {
            _logger.Log(Logger.LogType.ERROR, e.Message);
            throw;
        }

        return _responseList;
    }

    private static async Task<(string downloadLink, string size, string date, string version)> GetDataFromLink(string _link)
    {
        using var _httpClient = new HttpClient();
        var _html = await _httpClient.GetStringAsync(_link);
        var _htmlDocument = new HtmlAgilityPack.HtmlDocument();
        _htmlDocument.LoadHtml(_html);

        var _downloadLink = _htmlDocument.DocumentNode
            .SelectSingleNode("//a[contains(@class, 'download-btn')]").Attributes["href"].Value;
        
        var _version = _htmlDocument.DocumentNode
            .SelectSingleNode("//a[contains(@class, 'download-btn')]").InnerText.Trim();

        string pattern = @"\b(?:v|version)?\s*(\d+(?:\.\d+){0,2})\b";;
        Match match = Regex.Match(_version, pattern);
        string _ver = string.Empty;
        if (match.Success)
        {
            _ver = match.Value;
        }
        
        var _size = _htmlDocument.DocumentNode
            .SelectSingleNode("//div[contains(@class, 'inside-article')]/div[1]/p[6]/em").InnerText
            .Trim();

        var dateNode = _htmlDocument.DocumentNode.SelectSingleNode("//p//text()[contains(., 'Release date:')]");
        string date = string.Empty;
        if (dateNode != null)
        {
            date = dateNode.InnerText.Replace("Release date:", "").Trim();
        }
        
        return (_downloadLink, _size, date, _ver);
    }

    private static string BypassDownloadLink(string url)
    {
        using var _client = new HttpClient();
        var _htmlString = _client.GetStringAsync(url).Result;
        var _document = new HtmlAgilityPack.HtmlDocument();
        _document.LoadHtml(_htmlString);
        return _document.DocumentNode.SelectSingleNode("//input")
            .Attributes["value"].Value.Replace("&amp;", "&").Replace("&#038;", "&");
    }
    
    private static bool IsGameMatch(string gameName, string searchQuery)
    {
        string NormalizeForComparison(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            
            input = input.ToLower().Trim();
            input = Regex.Replace(input, @"[^\w\s]", " ");
            input = Regex.Replace(input, @"\s+", " ");
            
            var commonWords = new[] { "repack", "goty", "edition", "complete", "collection" };
            foreach (var word in commonWords)
            {
                input = Regex.Replace(input, $@"\b{word}\b", "", RegexOptions.IgnoreCase);
            }
            
            return input.Trim();
        }

        var normalizedGame = NormalizeForComparison(gameName);
        var normalizedQuery = NormalizeForComparison(searchQuery);
        
        if (string.IsNullOrWhiteSpace(normalizedGame) || string.IsNullOrWhiteSpace(normalizedQuery))
            return false;
        
        var gameWords = normalizedGame.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var queryWords = normalizedQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        return queryWords.All(queryWord => 
            gameWords.Any(gameWord => gameWord.Contains(queryWord) || queryWord.Contains(gameWord)));
    }
    
    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;
        
        var normalized = name
            .Replace("#8211;", " - ")
            .Replace("&#8211;", " - ")
            .Replace("&nbsp;", " ")
            .Replace("&amp;", " & ")
            .Replace("#038;", " & ")
            .Replace("&#8217;", "'")
            .Replace("&#039;", "'");
        
        normalized = Regex.Replace(normalized, @"\s+", " ");
        return normalized;
    }
}