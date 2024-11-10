using System.Drawing.Imaging;
using Dionysus.App.Models;
using Dionysus.Web;
using Newtonsoft.Json.Linq;

namespace Dionysus.App.Helpers;

public class ProfileData
{
    private static string _jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/profile.json");
    private static JObject _configJson;
    private static ProfileModel _profile = new();

    public static ProfileModel Profile() => _profile;

    public static void SelectImage()
    {
        using (OpenFileDialog openFileDialog = new OpenFileDialog())
        {
            openFileDialog.Filter =
                "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fullPath = openFileDialog.FileName;
                
                using (var image = new Bitmap(fullPath))
                {
                    if (image.Height != image.Width)
                    {
                        Toast.toast.ShowToast("The image is not of different width and height", "error");
                        return;
                    }
                    if (image.Height > 256 || image.Width > 256)
                    {
                        using (var resizedImage = new Bitmap(image, new Size(256, 256)))
                        {
                            string resizedPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/profileImage" + Path.GetExtension(fullPath));
                            resizedImage.Save(resizedPath);
                            Toast.toast.ShowToast("Image selected. Save settings", "info");
                            _profile.Image = resizedPath;
                        }
                    }
                    else
                    {
                        string resizedPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/profileImage" + Path.GetExtension(fullPath));
                        image.Save(resizedPath);
                        Toast.toast.ShowToast("Image selected. Save settings", "info");
                        _profile.Image = resizedPath;
                    }
                }
            }
            else
            {
                _profile.Image = null;
            }
        }
    }
    public static string GetBase64Image(string imagePath)
    {
        var imageBytes = System.IO.File.ReadAllBytes(imagePath);
        return $"data:image/jpeg;base64,{Convert.ToBase64String(imageBytes)}";
    }
    
    public static TimeSpan GetTotalPlayTime()
    {
        TimeSpan totalPlayTime = TimeSpan.Zero;

        foreach (var game in LibraryPage._gamesList)
        {
            var gameTime = LibraryPage.ParseTimeInfo(game.TimeInfo);
            totalPlayTime = totalPlayTime.Add(gameTime);
        }

        return totalPlayTime;
    }
    
    public static async Task InitializeProfileData()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_jsonPath));

        if (!File.Exists(_jsonPath))
        {
            _configJson = new JObject
            {
                ["user_name"] = "user_" + new Random().Next(10000, 100000),
                ["profile_image"] = ""
            };
            await File.WriteAllTextAsync(_jsonPath, _configJson.ToString());
        }
        else
        {
            var json = await File.ReadAllTextAsync(_jsonPath);
            _configJson = JObject.Parse(json);
        }

        _profile.Username = _configJson["user_name"].Value<string>();
        _profile.Image = _configJson["profile_image"].Value<string>();
    }
    public static void SaveJson()
    {
        _configJson["user_name"] = _profile.Username;
        _configJson["profile_image"] = _profile.Image;
        File.WriteAllText(_jsonPath, _configJson.ToString());
    }
}