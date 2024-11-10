using System.Diagnostics;
using Dionysus.App.Data;
using Dionysus.App.Models;

namespace Dionysus.App.Helpers;

public class GamesMonitor
{
    /// <summary>
    /// Tracking functions
    /// </summary>
    private static bool GameIsRunning(string gameName) => Process.GetProcessesByName(gameName).Length > 0;

    private static Logger.Logger _logger = new Logger.Logger();

    public static (bool isRunning, GameModel runningGame) GameFromListIsRunning(List<GameModel> gamesList)
    {
        if (gamesList == null || gamesList.Count == 0)
            return (false, null);

        foreach (var game in gamesList)
        {
            if (GameIsRunning(Path.GetFileNameWithoutExtension(game.Location)))
            {
                return (true, game);
            }
        }

        return (false, null);
    }

    public static bool GameIsDeletedFromDesktop(GameModel game) => !File.Exists(game.Location);
    
    /*public static void RemoveDeletedGames(List<GameModel> gamesList)
    {
        if (gamesList == null) return;
        gamesList.RemoveAll(game => !File.Exists(game.Location));
        GameData.GamesData.SaveToJSON(gamesList);
    }*/


    /// <summary>
    /// Time counter functions
    /// </summary>

    #region variables

    private static int hours = 0;

    private static int minutes = 0;
    private static int seconds = 0;
    public static bool isCounting = false;

    #endregion

    public static async Task CountPlayTime(List<GameModel> gamesList, string gameLocation)
    {
        var game = gamesList.FirstOrDefault(g => g.Location == gameLocation);
        if (game != null)
        {
            _logger.Log(Logger.Logger.LogType.DEBUG, $"Starting to count time for game: {game.Title} at {DateTime.Now}");

            ResetTime();
            ParseTimeInfo(game.TimeInfo);

            while (isCounting)
            {
                await Task.Delay(1000);
                IncrementTime();

                game.TimeInfo = $"{hours}h {minutes}m";
                _logger.Log(Logger.Logger.LogType.DEBUG, $"{game.Title} - Time: {hours}h {minutes}m {seconds}s");
            }
            _logger.Log(Logger.Logger.LogType.DEBUG, $"Game closed, saving final time for {game.Title} at {DateTime.Now}");
            await GameData.GamesData.SaveToJSON(gamesList);
        }
        else
        {
            _logger.Log(Logger.Logger.LogType.ERROR,$"Game with location {gameLocation} not found in the list.");
        }
        
        isCounting = false;
        
    }

    private static void ResetTime()
    {
        _logger.Log(Logger.Logger.LogType.DEBUG,"Resetting time to 0.");
        hours = 0;
        minutes = 0;
        seconds = 0;
    }

    private static void ParseTimeInfo(string timeInfo)
    {
        _logger.Log(Logger.Logger.LogType.DEBUG,$"Parsing time info: {timeInfo}");

        if (!string.IsNullOrEmpty(timeInfo))
        {
            var timeComponents = timeInfo.Split(' ');
            if (timeComponents.Length == 2)
            {
                if (timeComponents[0].EndsWith("h"))
                {
                    int.TryParse(timeComponents[0].Replace("h", ""), out hours);
                    _logger.Log(Logger.Logger.LogType.DEBUG,$"Parsed hours: {hours}");
                }

                if (timeComponents[1].EndsWith("m"))
                {
                    int.TryParse(timeComponents[1].Replace("m", ""), out minutes);
                    _logger.Log(Logger.Logger.LogType.DEBUG,$"Parsed minutes: {minutes}");
                }
            }
        }
        else
        {
            _logger.Log(Logger.Logger.LogType.DEBUG," No time info available, resetting time.");
            ResetTime();
        }
    }

    private static void IncrementTime()
    {
        seconds++;
        if (seconds == 60)
        {
            seconds = 0;
            minutes++;
            _logger.Log(Logger.Logger.LogType.DEBUG,$"Incremented minutes: {minutes}");
        }

        if (minutes == 60)
        {
            minutes = 0;
            hours++;
            _logger.Log(Logger.Logger.LogType.DEBUG,$"Incremented hours: {hours}");
        }
    }
}