namespace Dionysus.App.Models;

public class GameModel
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string TimeInfo { get; set; }
    public string ImageUrl { get; set; }
    public string Location { get; set; }
    public string Arguments { get; set; }
    public string LastRun { get; set; }
}