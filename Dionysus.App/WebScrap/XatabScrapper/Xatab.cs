using System.Net;
using System.Text.RegularExpressions;
using craftersmine.SteamGridDBNet;
using Dionysus.App.Data;
using Dionysus.App.Logger;
using Dionysus.Web;

namespace Dionysus.WebScrap.XatabScrapper;

public class Xatab
{
    private static readonly string _baseLink = "https://byxatab.com";
    private static readonly HttpClient _httpClient;
    private static readonly Logger _logger = new();
    
    static Xatab()
    {
        var handler = new HttpClientHandler
        {
            UseCookies = true,
            CookieContainer = new CookieContainer(),
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };
        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
    }

    public static async Task<bool> GetStatus()
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(_baseLink);
            if (response.IsSuccessStatusCode)
            {
                _logger.Log(Logger.LogType.DEBUG,$"Website {_baseLink} is available. Status: {response.StatusCode}");
                return true;
            }
            else
            {
                _logger.Log(Logger.LogType.DEBUG,$"Website {_baseLink} is unavailable. Status: {response.StatusCode}");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.Log(Logger.LogType.ERROR,ex.Message);
            return false;
        }
    }

    public static async Task<IEnumerable<SearchGameInfoStruct>> GetSearchResponse(string request)
    {
        var responseList = new List<SearchGameInfoStruct>();
        request = request.Length > 15 ? request.Substring(0, 15) : request;
        try
        {
            var initialLink = $"{_baseLink}/search/{request}";
            var initialHtml = await _httpClient.GetStringAsync(initialLink);
            var initialDocument = new HtmlAgilityPack.HtmlDocument();
            initialDocument.LoadHtml(initialHtml);

            var pageLinks = initialDocument.DocumentNode.SelectNodes("//div[@class='pages']//a");
            int pageCount = pageLinks?.Select(link => int.Parse(link.InnerText)).Max() ?? 1;

            var tasks = new List<Task<List<SearchGameInfoStruct>>>();
            for (int page = 0; page <= pageCount; page++)
            {
                tasks.Add(ProcessPage(page, request));
            }

            var results = await Task.WhenAll(tasks);
            return results.SelectMany(x => x).Where(x => x.Name != null);
        }
        catch (Exception e)
        {
            _logger.Log(Logger.LogType.ERROR, e.Message);
            throw;
        }
    }

    private static async Task<List<SearchGameInfoStruct>> ProcessPage(int page, string request)
    {
        var pageResults = new List<SearchGameInfoStruct>();
        try
        {
            var siteLink = $"{_baseLink}/search/{request}/page/{page}";
            var html = await _httpClient.GetStringAsync(siteLink);
            var htmlDocument = new HtmlAgilityPack.HtmlDocument();
            htmlDocument.LoadHtml(html);

            var responseDivs = htmlDocument.DocumentNode.SelectNodes("//div[@class='entry']");
            if (responseDivs == null) return pageResults;

            var gameTasks = responseDivs.Select(div => ProcessGameEntry(div, request));
            var games = await Task.WhenAll(gameTasks);
            
            pageResults.AddRange(games.Where(entry => entry.Name != null));
        }
        catch (Exception ex)
        {
            _logger.Log(Logger.LogType.ERROR, $"Error processing page {page}: {ex.Message}");
        }
        return pageResults;
    }

    private static async Task<SearchGameInfoStruct> ProcessGameEntry(HtmlAgilityPack.HtmlNode div, string request)
    {
        try
        {
            var titleNode = div.SelectSingleNode(".//div[@class='entry__title h2']/a");
            if (titleNode == null) return new SearchGameInfoStruct();

            var gameLink = titleNode.Attributes["href"]?.Value;
            var title = titleNode.InnerText?.Trim();
            
            if (string.IsNullOrEmpty(gameLink) || string.IsNullOrEmpty(title) || title.Contains("Decepticon"))
                return new SearchGameInfoStruct();

            var rephrasedName = NormalizeName(title);
            if (!IsGameMatch(rephrasedName, request)) return new SearchGameInfoStruct();

            var gameDataTask = GetDataFromLink(gameLink);
            var coverTask = SteamGridDB.GetGridUriHorizontal(rephrasedName);

            await Task.WhenAll(gameDataTask, coverTask);
            var (downloadLink, size, version) = gameDataTask.Result;

            var _date = div.SelectSingleNode(".//div[3]/div").InnerText.Split(",")[0].Trim();
            
            return new SearchGameInfoStruct
            {
                Name = title.Replace("&#039;", "'")
                    .Replace("Папка игры", "Game folder")
                    .Replace("Лицензия", "License")
                    .Replace("Архив","Archive"),
                Link = gameLink,
                Size = size.Replace("Гб", "GB").Replace("гб", "GB").Replace("ГБ","GB"),
                DownloadLink = downloadLink,
                Version = version,
                Date = _date.Replace("-",".").Replace("Вчера", "Yesterday").Replace("Сегодня", "Today")
            };
        }
        catch (Exception ex)
        {
            _logger.Log(Logger.LogType.ERROR, $"Error processing game entry: {ex.Message}");
            return new SearchGameInfoStruct();
        }
    }

    private static async Task<(string downloadLink, string size, string version)> GetDataFromLink(string link)
    {
        var html = await _httpClient.GetStringAsync(link);
        var htmlDocument = new HtmlAgilityPack.HtmlDocument();
        htmlDocument.LoadHtml(html);

        var downloadLink = htmlDocument.DocumentNode
            .SelectSingleNode("//a[contains(@class, 'download-torrent')]")?
            .GetAttributeValue("href", string.Empty) ?? string.Empty;

        var size = htmlDocument.DocumentNode
            .SelectSingleNode("//span[@class='entry__info-size']")?
            .InnerText.Trim() ?? string.Empty;
        var version = ExtractVersion(htmlDocument);

        return (downloadLink, size, version);
    }

    private static string ExtractVersion(HtmlAgilityPack.HtmlDocument doc)
    {
        var versionPaths = new[]
        {
            "//b[contains(text(), 'Версия игры')]",
            "//span[contains(text(), 'Версия игры')]",
            "//div[contains(@class, 'inner-entry__content-text')]/span"
        };

        foreach (var xpath in versionPaths)
        {
            var node = doc.DocumentNode.SelectSingleNode(xpath);
            if (node != null)
            {
                var version = node.InnerText
                    .Replace("Версия игры: ", "")
                    .Replace("- Версия игры: ", "")
                    .Replace("&nbsp;", " ")
                    .Trim();

                var match = Regex.Match(version, @"\d+(\.\d+)+");
                if (match.Success)
                    return match.Value;
            }
        }
        return string.Empty;
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