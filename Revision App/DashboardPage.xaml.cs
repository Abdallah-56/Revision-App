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
        private enum DashboardSection { Home, Revision, Progress }
        private DashboardSection activeSection = DashboardSection.Home;

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

        private async void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSection(DashboardSection.Home);
            await LoadDashboardAsync();
        }

        private void RevisionButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSection(DashboardSection.Revision);
        }

        private void ProgressButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to a fresh page so the progress figures are reloaded.
            ProgressContent.Navigate(new ProgressPage());
            ShowSection(DashboardSection.Progress);
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

        // Fade only the content area; the sidebar and header stay fixed.
        private void ShowSection(DashboardSection section)
        {
            if (activeSection == section)
                return;

            UIElement outgoing = GetSectionElement(activeSection);
            UIElement incoming = GetSectionElement(section);
            activeSection = section;

            incoming.BeginAnimation(UIElement.OpacityProperty, null);
            outgoing.BeginAnimation(UIElement.OpacityProperty, null);
            incoming.Opacity = 0;
            outgoing.Opacity = 1;
            incoming.Visibility = Visibility.Visible;
            incoming.IsHitTestVisible = true;
            outgoing.IsHitTestVisible = false;

            Panel.SetZIndex(incoming, 1);
            Panel.SetZIndex(outgoing, 0);

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180));
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(180));
            fadeOut.Completed += (s, e) =>
            {
                if (activeSection != section)
                    return;
                outgoing.Visibility = Visibility.Collapsed;
                outgoing.BeginAnimation(UIElement.OpacityProperty, null);
                outgoing.Opacity = 0;
            };

            incoming.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            outgoing.BeginAnimation(UIElement.OpacityProperty, fadeOut);

            PageTitleText.Text = section switch
            {
                DashboardSection.Revision => "Revision",
                DashboardSection.Progress => "Progress",
                _ => "Dashboard"
            };

            UpdateNavigationButton(HomeButton, section == DashboardSection.Home);
            UpdateNavigationButton(RevisionButton, section == DashboardSection.Revision);
            UpdateNavigationButton(ProgressButton, section == DashboardSection.Progress);
        }

        private UIElement GetSectionElement(DashboardSection section)
        {
            return section switch
            {
                DashboardSection.Revision => RevisionContent,
                DashboardSection.Progress => ProgressContent,
                _ => HomeContent
            };
        }

        private void UpdateNavigationButton(Button button, bool selected)
        {
            button.Background = selected
                ? (Brush)FindResource("LightPurple") : Brushes.Transparent;
            button.Foreground = (Brush)FindResource(selected ? "DarkPurple" : "TextColour");
            button.FontWeight = selected ? FontWeights.Bold : FontWeights.Normal;
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

                // Quiz history drives these two dashboard statistics.
                // If history cannot load, still show the signed-in user's profile and XP.
                try
                {
                    var progress = await new ProgressService().GetProgressAsync();
                    TopicsText.Text = progress.TopicsStudied.ToString();
                    AverageScoreText.Text = progress.AverageScore + "%";
                }
                catch
                {
                    TopicsText.Text = "—";
                    AverageScoreText.Text = "—";
                }

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
