using craftersmine.SteamGridDBNet;
using Dionysus.App.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dionysus.App.Data;

public class SteamGridDB
{
    public static string _link = "https://www.steamgriddb.com/";
    private static string _jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "env.json");
    public static JObject configJson = (JObject)JsonConvert.DeserializeObject(System.IO.File.ReadAllText(_jsonPath));
    private static string steamGDBAPI = configJson["steamGDBAPI"].Value<string>();
    static SteamGridDb _steamGridDb = new SteamGridDb(steamGDBAPI);
    private static Logger.Logger _logger = new Logger.Logger();
    public static async Task<string?> GetGridUriHorizontal(string gameName)
    {
        if (string.IsNullOrWhiteSpace(gameName))
        {
            _logger.Log(Logger.Logger.LogType.ERROR, "Game name cannot be null or empty");
            return null;
        }
        
        try
        {
            var gameResults = await _steamGridDb.SearchForGamesAsync(gameName);
            
            if (gameResults == null || !gameResults.Any())
            {
                _logger.Log(Logger.Logger.LogType.ERROR, $"No games found for: {gameName}");
                return null;
            }

            foreach (var game in gameResults)
            {
                var images = await _steamGridDb.GetGridsForGameAsync(
                    game, 
                    dimensions: SteamGridDbDimensions.W920H430, 
                    types: SteamGridDbTypes.Static
                );

                if (images != null && images.Any())
                {
                    var imageUrl = images.FirstOrDefault()?.FullImageUrl;
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        return imageUrl;
                    }
                }

                _logger.Log(
                    Logger.Logger.LogType.ERROR,
                    $"No image found for game: {game.Name}"
                );
                return null;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.Log(
                Logger.Logger.LogType.ERROR,
                $"Error fetching grid URI for {gameName}: {ex.Message}"
            );
            return null;
        }
    }
    
    public static async Task<string?> GetGridUriVertical(string gameName)
    {
        if (string.IsNullOrWhiteSpace(gameName))
        {
            _logger.Log(Logger.Logger.LogType.ERROR, "Game name cannot be null or empty");
            return null;
        }
        
        try
        {
            var gameResults = await _steamGridDb.SearchForGamesAsync(gameName);
            
            if (gameResults == null || !gameResults.Any())
            {
                _logger.Log(Logger.Logger.LogType.ERROR, $"No games found for: {gameName}");
                return null;
            }

            foreach (var game in gameResults)
            {
                var images = await _steamGridDb.GetGridsForGameAsync(
                    game, 
                    dimensions: SteamGridDbDimensions.W600H900, 
                    types: SteamGridDbTypes.Static
                );

                if (images != null && images.Any())
                {
                    var imageUrl = images.FirstOrDefault()?.FullImageUrl;
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        return imageUrl;
                    }
                }

                _logger.Log(
                    Logger.Logger.LogType.ERROR,
                    $"No image found for game: {game.Name}"
                );
                return null;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.Log(
                Logger.Logger.LogType.ERROR,
                $"Error fetching grid URI for {gameName}: {ex.Message}"
            );
            return null;
        }
    }

}