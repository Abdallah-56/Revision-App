
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Revision_App.Models;
using Revision_App.Services;

namespace Revision_App
{
    public partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            InitializeComponent();
            Loaded += DashboardPage_Loaded;
        }

        private async void DashboardPage_Loaded(
            object sender, RoutedEventArgs e)
        {
            await LoadDashboardAsync();
        }

        private async void RefreshButton_Click(
            object sender, RoutedEventArgs e)
        {
            await LoadDashboardAsync();
        }

        private async Task LoadDashboardAsync()
        {
            StatusText.Text = "Loading your profile...";
            XPText.Text = "—";
            FullNameText.Text = "Loading...";
            EmailText.Text = "Loading...";

            try
            {
                var client = SupabaseService.Client;

                if (client == null)
                {
                    StatusText.Text = "Supabase is not initialised.";
                    return;
                }


                var authUser = SupabaseService.Client!.Auth.CurrentUser;

                if (authUser == null)
                {
                    StatusText.Text = "No active login session. Please log in again.";
                    return;
                }

                string authUserId = authUser.Id;

                var response = await SupabaseService.Client
                    .From<User>()
                    .Where(user => user.AuthUserID == authUserId)
                    .Get();

                var profile = response.Models.FirstOrDefault();

                if (profile == null)
                {
                    StatusText.Text = "Profile not returned by Supabase.";
                    MessageBox.Show(
                        $"The profile query returned no matching rows.\n" +
                        $"Authenticated user ID: {authUserId}");
                    return;
                }

                // Display the user's saved profile information.
                string fullName =
                    $"{profile.FirstName} {profile.LastName}".Trim();

                FullNameText.Text = fullName;
                EmailText.Text = profile.Email;
                XPText.Text = profile.XP.ToString();

                WelcomeText.Text = string.IsNullOrWhiteSpace(fullName)
                    ? "Welcome back"
                    : $"Welcome back, {profile.FirstName}";

                // These statistics need quiz/progress data before
                // they can be calculated accurately.
                TopicsText.Text = "Not available yet";
                AverageScoreText.Text = "Not available yet";

                StatusText.Text = "Dashboard loaded successfully.";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Could not load dashboard data.";

                MessageBox.Show(
                    $"Unable to load your dashboard:\n{ex.Message}",
                    "Dashboard error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
