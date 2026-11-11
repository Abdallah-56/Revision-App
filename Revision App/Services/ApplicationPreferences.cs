using System;
using System.IO;
using System.Text.Json;

namespace Revision_App.Services
{
    public class ApplicationPreferences
    {
        public bool ShowRevisionReminders { get; set; } = true;
        public int DailyQuizGoal { get; set; } = 2;

        private static string GetFilePath()
        {
            string? authId = SupabaseService.Client?.Auth.CurrentUser?.Id;
            if (!Guid.TryParse(authId, out var parsedId))
                throw new InvalidOperationException("Sign in before changing settings.");

            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CompRev");
            return Path.Combine(folder, $"preferences-{parsedId:D}.json");
        }

        public static ApplicationPreferences LoadForCurrentUser()
        {
            string path = GetFilePath();
            if (!File.Exists(path))
                return new ApplicationPreferences();

            string content = File.ReadAllText(path);
            var preferences = JsonSerializer.Deserialize<ApplicationPreferences>(content)
                ?? new ApplicationPreferences();
            if (preferences.DailyQuizGoal is not (1 or 2 or 3 or 5))
                preferences.DailyQuizGoal = 2;
            return preferences;
        }

        public void SaveForCurrentUser()
        {
            if (DailyQuizGoal is not (1 or 2 or 3 or 5))
                throw new ArgumentOutOfRangeException(nameof(DailyQuizGoal));

            string path = GetFilePath();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(this,
                new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
