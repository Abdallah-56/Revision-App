
using System;
using System.Windows;
using System.Windows.Controls;

namespace Revision_App
{
    public partial class RevisionTopicsPage : UserControl
    {
        public RevisionTopicsPage()
        {
            InitializeComponent();
        }

        // Filter topics using the search box.
        private void SearchBox_TextChanged(
            object sender, TextChangedEventArgs e)
        {
            if (TopicGrid == null || NoResultsText == null)
                return;

            string search = SearchBox.Text.Trim();
            int matches = 0;

            foreach (Border card in TopicGrid.Children)
            {
                string topic = card.Tag?.ToString() ?? "";

                bool visible = topic.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase);

                card.Visibility = visible
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                if (visible)
                    matches++;
            }

            NoResultsText.Visibility = matches == 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        // Handle the selected revision topic.
        private void StartRevision_Click(
            object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            string topic = button.Tag?.ToString()
                ?? "Unknown topic";

            MessageBox.Show(
                $"You selected: {topic}\n\n" +
                "Revision content is not added yet.",
                "CompRev",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
