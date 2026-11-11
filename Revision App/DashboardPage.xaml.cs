using System;
using System.Linq;
using System.Collections.Generic;
using Supabase.Gotrue;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using Revision_App.Models;
using Revision_App.Services;

namespace Revision_App
{
    public partial class DashboardPage : Page
    {
        private enum DashboardSection { Home, Revision, Progress, Settings }
        private DashboardSection activeSection = DashboardSection.Home;
        private List<RevisionRecommendation> currentRecommendations = new();
        private bool isLoggingOut;

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

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            OpenSettings();
        }

        private void AccountButton_Click(object sender, RoutedEventArgs e)
        {
            OpenSettings();
        }

        private void OpenSettings()
        {
            var page = new SettingsPage();
            page.LogOutRequested += SettingsPage_LogOutRequested;
            SettingsContent.Navigate(page);
            ShowSection(DashboardSection.Settings);
        }

        private void NotificationsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var preferences = ApplicationPreferences.LoadForCurrentUser();
                if (!preferences.ShowRevisionReminders)
                {
                    MessageBox.Show("Revision reminders are disabled. Enable them in Settings.",
                        "Notifications", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (currentRecommendations.Count == 0)
                {
                    MessageBox.Show("No recommendations are available yet. Try refreshing the dashboard.",
                        "Notifications", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string items = string.Join("\n", currentRecommendations.Select(r =>
                    $"• {r.TopicName}: {r.Reason}"));
                var result = MessageBox.Show(
                    "Your revision reminders:\n\n" + items +
                    "\n\nOpen your Progress page for more information?",
                    "Revision Reminders", MessageBoxButton.YesNo,
                    MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                    ProgressButton_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load reminders: " + ex.Message,
                    "Notifications", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void SettingsPage_LogOutRequested(object? sender, EventArgs e)
        {
            await LogOutAsync();
        }

        private async void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            await LogOutAsync();
        }

        private async Task LogOutAsync()
        {
            if (isLoggingOut)
                return;

            if (MessageBox.Show("Log out of CompRev?", "Confirm Log Out",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            isLoggingOut = true;
            LogOutButton.IsEnabled = false;
            try
            {
                var client = SupabaseService.Client
                    ?? throw new InvalidOperationException("Supabase is not initialised.");
                var navigation = NavigationService
                    ?? throw new InvalidOperationException("Dashboard navigation is unavailable.");

                // Only sign out of this device, not the user's other devices.
                await client.Auth.SignOut(Constants.SignOutScope.Local);

                // Remove authenticated pages after the login page has loaded.
                NavigatedEventHandler? onNavigated = null;
                onNavigated = (source, args) =>
                {
                    if (args.Content is not LogInPage)
                        return;

                    navigation.Navigated -= onNavigated!;
                    while (navigation.RemoveBackEntry() != null) { }
                };
                navigation.Navigated += onNavigated;
                if (!navigation.Navigate(new LogInPage()))
                {
                    navigation.Navigated -= onNavigated;
                    throw new InvalidOperationException("Could not open the login page.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not finish logging out: " + ex.Message,
                    "Log Out", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                isLoggingOut = false;
                LogOutButton.IsEnabled = true;
            }
        }

        private void StartRecommendedQuiz_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string topicName)
            {
                RevisionContent.Navigate(new QuizPage(topicName));
                ShowSection(DashboardSection.Revision);
            }
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
                DashboardSection.Settings => "Settings",
                _ => "Dashboard"
            };

            UpdateNavigationButton(HomeButton, section == DashboardSection.Home);
            UpdateNavigationButton(RevisionButton, section == DashboardSection.Revision);
            UpdateNavigationButton(ProgressButton, section == DashboardSection.Progress);
            UpdateNavigationButton(SettingsButton, section == DashboardSection.Settings);
        }

        private UIElement GetSectionElement(DashboardSection section)
        {
            return section switch
            {
                DashboardSection.Revision => RevisionContent,
                DashboardSection.Progress => ProgressContent,
                DashboardSection.Settings => SettingsContent,
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

        private void ApplyDailyGoal(int completedToday)
        {
            try
            {
                var preferences = ApplicationPreferences.LoadForCurrentUser();
                int goal = preferences.DailyQuizGoal;
                DailyGoalProgress.Maximum = goal;
                DailyGoalProgress.Value = Math.Min(goal, completedToday);
                DailyGoalCountText.Text = $"{completedToday} / {goal} quizzes";
                DailyGoalDetailText.Text = completedToday >= goal
                    ? "Today's quiz goal completed."
                    : "Complete quizzes today to reach your goal. Change the target in Settings.";
            }
            catch
            {
                DailyGoalDetailText.Text = "Unable to load today's quiz goal.";
            }
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
                    .From<Models.User>()
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

                // Use the same progress snapshot for both statistics and suggestions.
                try
                {
                    var progress = await new ProgressService().GetProgressAsync();
                    TopicsText.Text = progress.TopicsStudied.ToString();
                    AverageScoreText.Text = progress.AverageScore + "%";

                    var recommendations = new AdaptiveRevisionService()
                        .GetRecommendations(progress, 3);
                    RecommendationItems.ItemsSource = recommendations;
                    currentRecommendations = recommendations;
                    ApplyDailyGoal(progress.QuizzesToday);
                    NoRecommendationsText.Text = recommendations.Count == 0
                        ? "No revision topics are available."
                        : "";
                    NoRecommendationsText.Visibility = recommendations.Count == 0
                        ? Visibility.Visible : Visibility.Collapsed;
                }
                catch
                {
                    TopicsText.Text = "—";
                    AverageScoreText.Text = "—";
                    RecommendationItems.ItemsSource = null;
                    currentRecommendations.Clear();
                    NoRecommendationsText.Text = "Recommendations are unavailable. Please refresh.";
                    NoRecommendationsText.Visibility = Visibility.Visible;
                }

                try
                {
                    var activity = await new RecentActivityService().GetRecentActivityAsync(4);
                    RecentActivityItems.ItemsSource = activity;
                    NoActivityText.Text = "No completed quizzes yet.";
                    NoActivityText.Visibility = activity.Count == 0
                        ? Visibility.Visible : Visibility.Collapsed;
                }
                catch
                {
                    RecentActivityItems.ItemsSource = null;
                    NoActivityText.Text = "Recent activity is unavailable. Please refresh.";
                    NoActivityText.Visibility = Visibility.Visible;
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