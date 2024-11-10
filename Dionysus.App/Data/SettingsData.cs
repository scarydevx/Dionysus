using Dionysus.App.Models;
using Newtonsoft.Json;

namespace Dionysus.App.Data;

public class SettingsData
{
    private static string _jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/settings.json");
    public static async Task SaveToJSON(List<SettingsModel> settings)
    {
        var directory = Path.GetDirectoryName(_jsonPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);

        }
        var settingsWrapper = new { Settings = settings };
    
        var json = JsonConvert.SerializeObject(settingsWrapper, Formatting.Indented);
    
        await File.WriteAllTextAsync(_jsonPath, json);

        await Task.CompletedTask;
    }
    
    public static async Task<List<SettingsModel>> LoadFromJSON()
    {
        if (File.Exists(_jsonPath))
        {
            var json = await File.ReadAllTextAsync(_jsonPath);
            var settingsWrapper = JsonConvert.DeserializeObject<SettingsWrapper>(json);
            return settingsWrapper.Settings;
        }
        
        return new List<SettingsModel>();
    }
}

public class SettingsWrapper
{
    public List<SettingsModel> Settings { get; set; }
}