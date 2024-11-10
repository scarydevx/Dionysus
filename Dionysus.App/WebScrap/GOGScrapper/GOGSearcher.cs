using Dionysus.App.Interfaces;

namespace Dionysus.WebScrap.GOGScrapper;

public class GOGSearcher : IGameSearcher
{
    public string SourceName => "GOG";

    public async Task<IEnumerable<SearchGameInfoStruct>> Search(string gameName)
    {
        return await GOG.GetSearchResponse(gameName);
    }
}