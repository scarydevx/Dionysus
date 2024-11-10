using Dionysus.App.Interfaces;

namespace Dionysus.WebScrap.FitGirlScrapper;

public class FitGirlSearcher : IGameSearcher
{
    public string SourceName => "FitGirl";

    public async Task<IEnumerable<SearchGameInfoStruct>> Search(string gameName)
    {
        return await FitGirl.GetSearchResponse(gameName);
    }
}