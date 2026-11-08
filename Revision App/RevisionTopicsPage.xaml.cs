using System;
using System.Windows;
using System.Windows.Controls;

namespace Revision_App
{
    public partial class RevisionTopicsPage : Page
    {
        public RevisionTopicsPage()
        {
            InitializeComponent();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TopicGrid == null || NoResultsText == null)
                return;

            string searchText = SearchBox.Text.Trim();
            int visibleTopics = 0;

            foreach (Border card in TopicGrid.Children)
            {
                string topicName = card.Tag?.ToString() ?? "";
                bool matches = topicName.Contains(searchText,
                    StringComparison.OrdinalIgnoreCase);

                card.Visibility = matches ? Visibility.Visible : Visibility.Collapsed;
                if (matches)
                    visibleTopics++;
            }

            NoResultsText.Visibility = visibleTopics == 0
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void StartRevision_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not string topicName)
                return;

            // Use the existing dashboard Frame to show the quiz.
            System.Windows.Navigation.NavigationService.GetNavigationService(this)?.Navigate(new QuizPage(topicName));
        }
    }
}

