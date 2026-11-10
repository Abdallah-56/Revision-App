using System;
using System.Windows;
using System.Windows.Controls;
using Revision_App.Services;

namespace Revision_App
{
    public partial class ProgressPage : Page
    {
        private readonly ProgressService progressService = new ProgressService();
        private bool loading;

        public ProgressPage()
        {
            InitializeComponent();
            Loaded += ProgressPage_Loaded;
        }

        private async void ProgressPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadProgressAsync();
        }

        private async void RefreshProgress_Click(object sender, RoutedEventArgs e)
        {
            await LoadProgressAsync();
        }

        private async System.Threading.Tasks.Task LoadProgressAsync()
        {
            if (loading)
                return;

            loading = true;
            StatusText.Text = "Loading your saved quiz results...";
            try
            {
                ProgressSnapshot progress = await progressService.GetProgressAsync();

                OverallText.Text = progress.OverallProgress + "%";
                XPText.Text = progress.XP.ToString();
                StreakText.Text = progress.DayStreak.ToString();
                QuizzesText.Text = progress.QuizzesCompleted.ToString();
                TopicItems.ItemsSource = progress.Topics;
                HistoryItems.ItemsSource = progress.History;
                var suggestions = new AdaptiveRevisionService()
                    .GetRecommendations(progress, 3);
                RecommendedTopicItems.ItemsSource = suggestions;
                NoRecommendedTopicsText.Visibility = suggestions.Count == 0
                    ? Visibility.Visible : Visibility.Collapsed;
                NoHistoryText.Visibility = progress.History.Count == 0
                    ? Visibility.Visible : Visibility.Collapsed;

                TopicsSummaryText.Text =
                    $"{progress.TopicsStudied} of {progress.TotalTopics} topics studied " +
                    $"· Average quiz score: {progress.AverageScore}%";

                StatusText.Text = progress.QuizzesCompleted == 0
                    ? "No saved quizzes yet. Complete a quiz to start tracking progress."
                    : "Quiz scores are saved; knowledge retention is estimated from your last revision date.";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Could not load progress: " + ex.Message;
            }
            finally
            {
                loading = false;
            }
        }
    }
}
