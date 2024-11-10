using Dionysus.App.Forms;

namespace Dionysus.App;

static class Program
{
    public static readonly string _appCodeName = "Lotus";
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new MainWindow());
    }
}



/*
 *var searchTasks = _searchers.Select(async searcher =>
        {
            try
            {
                var startTime = DateTime.Now;
                _logger.Log(Logger.LogType.DEBUG, $"Searcher {searcher.SourceName} started at {startTime}");
                var searchResults = await Steam250.GetSearchResponse(searchText);
                var endTime = DateTime.Now;
                _logger.Log(Logger.LogType.DEBUG, $" Searcher {searcher.SourceName} finished at {endTime}. Duration: {(endTime - startTime).TotalSeconds} seconds");
                if (searchResults != null && searchResults.Any())
                {
                    foreach (var game in searchResults)
                    {
                        if (!alreadyFoundGameNames.Contains(game.Name))
                        {
                            var tempGame = game;
                            tempGame.Source = searcher.SourceName;
                            foundGames.Add(tempGame);
                            alreadyFoundGameNames.Add(game.Name);
                        }
                    }

                    if (foundGames.Any())
                    {
                        _games = new List<Steam250Struct>(foundGames);
                        StateHasChanged();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log(Logger.LogType.ERROR, $"Error in searcher {searcher.SourceName}: {ex.Message}");
            }
        }).ToList();
 * 
 */