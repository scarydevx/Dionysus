namespace Dionysus.WebScrap.Steam250;

public struct Steam250SearchStruct
{
    public string Name { get; set; }
    public string Link { get; set; }
    public string Cover { get; set; }
    public List<string> Tags { get; set; }
    public string AppId { get; set; }
    public string Relevance { get; set; }
}