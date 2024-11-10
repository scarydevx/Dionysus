using Dionysus.WebScrap;

namespace Dionysus.App.Interfaces;

public interface IGameSearcher
{
    Task<IEnumerable<SearchGameInfoStruct>> Search(string gameName);
    string SourceName { get; }
}
