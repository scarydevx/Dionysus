using System.Text.RegularExpressions;
using Dionysus.App.Data;
using Dionysus.App.Logger;

namespace Dionysus.WebScrap.Steam250;

public class Steam250
{
    private static Logger _logger = new Logger();
    public static async Task<IEnumerable<Steam250SearchStruct>> GetSearchResponse(string _request)
    {
        var _link = $"https://club.steam250.com/search?q={_request}&o=1";
        var _list = new List<Steam250SearchStruct>();
        try
        {
            using var _httpClient = new HttpClient();
            var _html = await _httpClient.GetStringAsync(_link);
            var _htmlDocument = new HtmlAgilityPack.HtmlDocument();
            _htmlDocument.LoadHtml(_html);

            var _responseDivs =
                _htmlDocument.DocumentNode.SelectNodes("//section[@class='applist']/div");
            if (_responseDivs != null)
            {
                var tasks = _responseDivs.Select(async _div =>
                {
                    var _name = _div.SelectSingleNode(".//div[@class='appline']/span[1]/a");
                    var _link = _name.Attributes["href"].Value;
                    var rephrasedName = NormalizeName(_name.InnerText.Trim());
                    var tags = _div.SelectNodes(".//div[@class='appline']//a[contains(@class, 'tag g1')]");
                    var appid = _link.Replace("/app/", "");

                    if (tags == null || !tags.Any())
                    {
                        tags = _div.SelectNodes(".//div[@class='appline']//a[contains(@class, 'tag g2')]");
                    }

                    var rephrasedRequest = NormalizeName(_request);
                    var _cover = await SteamGridDB.GetGridUriHorizontal(rephrasedName);
                    if (_cover != null)
                    {
                        var tagList = tags?.Select(x => x.InnerText).ToList() ?? new List<string>();
                        _list.Add(new Steam250SearchStruct()
                        {
                            Cover = _cover,
                            Name = rephrasedName,
                            Link = _link,
                            Tags = tagList,
                            AppId = appid
                        });
                    }
                });

                await Task.WhenAll(tasks);
            }
        }
        catch (HttpRequestException x)
        {
            _logger.Log(Logger.LogType.ERROR, x.Message);
            throw new Exception("Too many requests, try later");
        }
        
        return _list;
    }
    
    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty; 
        }
        
        var normalized = name
            .Replace("#8211;", " - ")
            .Replace("&#8211;", " - ")
            .Replace("&nbsp;", " ")
            .Replace("&amp;", " & ")
            .Replace("#038;", " & ")
            .Replace("&#8217;", "'")
            .Replace("&#039;", "'")
            .Replace("™", "");
        
        normalized = Regex.Replace(normalized, @"\s+", " ");

        return normalized;
    }
}