using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Revision_App.Models;
using Revision_App.Services;

namespace Revision_App
{
    public partial class DashboardPage : Page
    {
        private bool showingRevision = false;

        public DashboardPage()
        {
            InitializeComponent();
            RevisionContent.Navigate(new RevisionTopicsPage());
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

        private void HomeButton_Click(
            object sender, RoutedEventArgs e)
        {
            ShowSection(false);
        }

        private void RevisionButton_Click(
            object sender, RoutedEventArgs e)
        {
            ShowSection(true);
        }

        private async void TestQuestions_Click(
            object sender, RoutedEventArgs e)
        {
            try
            {
                var questionService = new QuestionService();
                var questions = await questionService.GetQuestionsAsync(3);

                MessageBox.Show(
                    $"Successfully loaded {questions.Count} Algorithms questions.",
                    "Database Test",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading questions:\n{ex.Message}",
                    "Database Test",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Switch the main content without reloading the whole page.
        private void ShowSection(bool revision)
        {
            if (showingRevision == revision)
                return;

            showingRevision = revision;

            UIElement incoming = revision
                ? RevisionContent
                : HomeContent;

            UIElement outgoing = revision
                ? HomeContent
                : RevisionContent;

            incoming.BeginAnimation(
                UIElement.OpacityProperty, null);

            outgoing.BeginAnimation(
                UIElement.OpacityProperty, null);

            incoming.Opacity = 0;
            outgoing.Opacity = 1;

            incoming.Visibility = Visibility.Visible;
            incoming.IsHitTestVisible = true;
            outgoing.IsHitTestVisible = false;

            Panel.SetZIndex(incoming, 1);
            Panel.SetZIndex(outgoing, 0);

            // Fade between the two views.
            var fadeIn = new DoubleAnimation(
                0, 1, TimeSpan.FromMilliseconds(180));

            var fadeOut = new DoubleAnimation(
                1, 0, TimeSpan.FromMilliseconds(180));

            fadeOut.Completed += (s, e) =>
            {
                if (showingRevision != revision)
                    return;

                outgoing.Visibility = Visibility.Collapsed;

                outgoing.BeginAnimation(
                    UIElement.OpacityProperty, null);

                outgoing.Opacity = 0;
            };

            incoming.BeginAnimation(
                UIElement.OpacityProperty, fadeIn);

            outgoing.BeginAnimation(
                UIElement.OpacityProperty, fadeOut);

            // Update the page title and selected sidebar item.
            PageTitleText.Text = revision
                ? "Revision"
                : "Dashboard";

            var selected = (Brush)FindResource("LightPurple");
            var selectedText = (Brush)FindResource("DarkPurple");
            var normalText = (Brush)FindResource("TextColour");

            HomeButton.Background = revision
                ? Brushes.Transparent
                : selected;

            HomeButton.Foreground = revision
                ? normalText
                : selectedText;

            RevisionButton.Background = revision
                ? selected
                : Brushes.Transparent;

            RevisionButton.Foreground = revision
                ? selectedText
                : normalText;
        }

        // Load the signed-in user's dashboard information.
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

                var authUser = client.Auth.CurrentUser;

                if (authUser == null)
                {
                    StatusText.Text =
                        "No active login session. Please log in again.";
                    return;
                }

                var response = await client
                    .From<User>()
                    .Where(user => user.AuthUserID == authUser.Id)
                    .Get();

                var profile = response.Models.FirstOrDefault();

                if (profile == null)
                {
                    StatusText.Text =
                        "Profile not returned by Supabase.";
                    return;
                }

                FullNameText.Text =
                    $"{profile.FirstName} {profile.LastName}".Trim();

                EmailText.Text = profile.Email;
                XPText.Text = profile.XP.ToString();

                WelcomeText.Text =
                    $"Welcome back, {profile.FirstName}";

                // These statistics will be added later.
                TopicsText.Text = "Not available yet";
                AverageScoreText.Text = "Not available yet";

                StatusText.Text = "Your revision overview";
            }
            catch (Exception ex)
            {
                StatusText.Text =
                    "Could not load dashboard data.";

                MessageBox.Show(
                    $"Unable to load your dashboard:\n{ex.Message}",
                    "Dashboard error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
