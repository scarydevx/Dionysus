using Dionysus.App.Interfaces;

namespace Dionysus.WebScrap.XatabScrapper;

public class XatabSearcher : IGameSearcher
{
    public string SourceName => "Xatab";

    public async Task<IEnumerable<SearchGameInfoStruct>> Search(string gameName)
    {
        return await Xatab.GetSearchResponse(gameName);
    }
}