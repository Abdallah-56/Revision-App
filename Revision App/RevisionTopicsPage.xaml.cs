
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

        // Return to the dashboard.
        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new DashboardPage());
        }

        // Search for topics by name.
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchBox.Text.Trim().ToLower();
            int visibleTopics = 0;

            foreach (Border topic in TopicGrid.Children)
            {
                string topicName = topic.Tag.ToString()!.ToLower();

                if (topicName.Contains(searchText))
                {
                    topic.Visibility = Visibility.Visible;
                    visibleTopics++;
                }
                else
                {
                    topic.Visibility = Visibility.Collapsed;
                }
            }

            // Show a message if no topics match.
            NoResultsText.Visibility = visibleTopics == 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        // Select a topic to revise.
        private void StartRevision_Click(object sender, RoutedEventArgs e)
        {
            Button selectedButton = (Button)sender;
            string selectedTopic = selectedButton.Tag.ToString()!;

            MessageBox.Show(
                $"You selected: {selectedTopic}",
                "Topic Selected",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
