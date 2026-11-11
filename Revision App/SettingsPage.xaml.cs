using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Revision_App.Models;
using Revision_App.Services;

namespace Revision_App
{
    public partial class SettingsPage : Page
    {
        public event EventHandler? LogOutRequested;

        public SettingsPage()
        {
            InitializeComponent();
            Loaded += SettingsPage_Loaded;
        }

        private async void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var preferences = ApplicationPreferences.LoadForCurrentUser();
                RemindersCheckBox.IsChecked = preferences.ShowRevisionReminders;
                int[] options = { 1, 2, 3, 5 };
                int index = Array.IndexOf(options, preferences.DailyQuizGoal);
                GoalComboBox.SelectedIndex = index < 0 ? 1 : index;
            }
            catch (Exception ex)
            {
                PreferencesStatusText.Text = "Could not load preferences: " + ex.Message;
            }
            await LoadAccountAsync();
        }

        private async void RefreshAccount_Click(object sender, RoutedEventArgs e)
        {
            await LoadAccountAsync();
        }

        private async Task LoadAccountAsync()
        {
            AccountStatusText.Text = "Loading account...";
            try
            {
                var client = SupabaseService.Client
                    ?? throw new InvalidOperationException("Supabase is not initialised.");
                var signedInUser = client.Auth.CurrentUser
                    ?? throw new InvalidOperationException("Please log in again.");
                var response = await client.From<User>()
                    .Where(u => u.AuthUserID == signedInUser.Id).Get();
                var profile = response.Models.FirstOrDefault()
                    ?? throw new InvalidOperationException("Profile not found.");
                AccountNameText.Text = $"{profile.FirstName} {profile.LastName}".Trim();
                AccountEmailText.Text = profile.Email;
                AccountXPText.Text = profile.XP.ToString();
                AccountStatusText.Text = "";
            }
            catch (Exception ex)
            {
                AccountStatusText.Text = "Could not retrieve account: " + ex.Message;
            }
        }

        private void SavePreferences_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int[] choices = { 1, 2, 3, 5 };
                if (GoalComboBox.SelectedIndex < 0 || GoalComboBox.SelectedIndex >= choices.Length)
                {
                    PreferencesStatusText.Text = "Choose a daily quiz goal.";
                    return;
                }
                var prefs = new ApplicationPreferences
                {
                    ShowRevisionReminders = RemindersCheckBox.IsChecked == true,
                    DailyQuizGoal = choices[GoalComboBox.SelectedIndex]
                };
                prefs.SaveForCurrentUser();
                PreferencesStatusText.Text = "Preferences saved on this computer. Return Home to see the new goal.";
            }
            catch (Exception ex)
            {
                PreferencesStatusText.Text = "Could not save preferences: " + ex.Message;
            }
        }

        private void LogOutFromSettings_Click(object sender, RoutedEventArgs e)
        {
            LogOutRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
