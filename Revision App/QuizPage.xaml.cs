using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Revision_App.Models;
using Revision_App.Services;

namespace Revision_App
{
    public partial class QuizPage : Page
    {
        private readonly string topicName;
        private readonly QuestionService questionService = new QuestionService();
        private readonly QuizAttemptService quizAttemptService = new QuizAttemptService();
        private readonly List<QuizAnswer> submittedAnswers = new List<QuizAnswer>();

        private List<Question> questions = new List<Question>();
        private Guid attemptID = Guid.NewGuid();
        private int topicID;
        private int questionIndex = 0;
        private int correctCount = 0;
        private bool loaded = false;
        private bool isSaving = false;
        private bool isSaved = false;

        public QuizPage(string selectedTopic)
        {
            InitializeComponent();
            topicName = selectedTopic;
            TopicTitleText.Text = topicName + " Quiz";
            Loaded += QuizPage_Loaded;
        }

        private async void QuizPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (loaded)
                return;

            loaded = true;

            try
            {
                List<Topic> topics = await questionService.GetTopicsAsync();
                Topic? topic = topics.FirstOrDefault(t =>
                    string.Equals(t.TopicName?.Trim(), topicName.Trim(),
                        StringComparison.OrdinalIgnoreCase));

                if (topic == null)
                {
                    StatusText.Text = "This topic was not found in the database.";
                    return;
                }

                topicID = topic.TopicID;
                List<Question> databaseQuestions =
                    await questionService.GetQuestionsAsync(topicID);

                questions = databaseQuestions
                    .Where(q => string.Equals(q.QuestionType, "MultipleChoice",
                        StringComparison.OrdinalIgnoreCase))
                    .OrderBy(q => q.QuestionID)
                    .ToList();

                if (questions.Count == 0)
                {
                    StatusText.Text = "No multiple-choice questions were found for this topic.";
                    return;
                }

                StatusText.Visibility = Visibility.Collapsed;
                QuizPanel.Visibility = Visibility.Visible;
                ShowQuestion();
            }
            catch (Exception ex)
            {
                StatusText.Text = "Could not load the quiz: " + ex.Message;
            }
        }

        private void ShowQuestion()
        {
            Question question = questions[questionIndex];

            QuestionIndexText.Text = $"Question {questionIndex + 1} of {questions.Count}";
            ScoreText.Text = $"Correct: {correctCount}";
            DifficultyText.Text = $"Difficulty: {question.Difficulty}/5";
            QuestionTextBlock.Text = question.QuestionText;

            OptionAText.Text = "A. " + question.OptionA;
            OptionBText.Text = "B. " + question.OptionB;
            OptionCText.Text = "C. " + question.OptionC;
            OptionDText.Text = "D. " + question.OptionD;

            AnswerA.IsChecked = false;
            AnswerB.IsChecked = false;
            AnswerC.IsChecked = false;
            AnswerD.IsChecked = false;

            AnswerA.IsEnabled = true;
            AnswerB.IsEnabled = true;
            AnswerC.IsEnabled = true;
            AnswerD.IsEnabled = true;

            FeedbackPanel.Visibility = Visibility.Collapsed;
            SubmitButton.Visibility = Visibility.Visible;
            NextButton.Visibility = Visibility.Collapsed;
            NextButton.IsEnabled = true;
        }

        private void SubmitAnswer_Click(object sender, RoutedEventArgs e)
        {
            RadioButton? selected = new[] { AnswerA, AnswerB, AnswerC, AnswerD }
                .FirstOrDefault(option => option.IsChecked == true);

            if (selected == null)
            {
                MessageBox.Show("Please select an answer before continuing.",
                    "Select an Answer", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Question question = questions[questionIndex];
            string answer = selected.Tag?.ToString() ?? "";
            string correctOption = question.CorrectOption.Trim().ToUpperInvariant();
            bool isCorrect = answer == correctOption;

            submittedAnswers.Add(new QuizAnswer
            {
                QuestionID = question.QuestionID,
                SelectedOption = answer
            });

            if (isCorrect)
                correctCount++;

            FeedbackText.Text = isCorrect
                ? "Correct answer!"
                : $"Incorrect. Correct answer: {question.CorrectAnswer}";
            FeedbackText.Foreground = isCorrect ? Brushes.ForestGreen : Brushes.Firebrick;
            ExplanationText.Text = question.Explanation;
            FeedbackPanel.Visibility = Visibility.Visible;
            ScoreText.Text = $"Correct: {correctCount}";

            AnswerA.IsEnabled = false;
            AnswerB.IsEnabled = false;
            AnswerC.IsEnabled = false;
            AnswerD.IsEnabled = false;

            SubmitButton.Visibility = Visibility.Collapsed;
            NextButton.Content = questionIndex == questions.Count - 1
                ? "Finish Quiz" : "Next Question";
            NextButton.Visibility = Visibility.Visible;
        }

        private async void NextQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (questionIndex == questions.Count - 1)
            {
                await FinishQuizAsync();
                return;
            }

            questionIndex++;
            ShowQuestion();
        }

        private async System.Threading.Tasks.Task FinishQuizAsync()
        {
            if (isSaving || isSaved)
                return;

            isSaving = true;
            NextButton.IsEnabled = false;
            NextButton.Content = "Saving...";

            try
            {
                await quizAttemptService.SaveAttemptAsync(attemptID, topicID, submittedAnswers);
                isSaved = true;
                ShowResults();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "The quiz could not be saved. Your answers are still on this page. " +
                    "Try saving again.\n\n" + ex.Message,
                    "Save Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                NextButton.Content = "Retry Save";
            }
            finally
            {
                isSaving = false;
                NextButton.IsEnabled = true;
            }
        }

        private void ShowResults()
        {
            QuizPanel.Visibility = Visibility.Collapsed;
            ResultsPanel.Visibility = Visibility.Visible;

            int percentage = (int)Math.Round(
                correctCount * 100.0 / questions.Count,
                MidpointRounding.AwayFromZero);

            ResultScoreText.Text = $"{correctCount} / {questions.Count} ({percentage}%)";
            ResultDetailText.Text =
                $"Results saved to Supabase. You earned {correctCount * 10} XP. " +
                "Use Refresh Dashboard to view your updated total XP.";
        }

        private void RestartQuiz_Click(object sender, RoutedEventArgs e)
        {
            attemptID = Guid.NewGuid();
            submittedAnswers.Clear();
            questionIndex = 0;
            correctCount = 0;
            isSaved = false;
            isSaving = false;

            ResultsPanel.Visibility = Visibility.Collapsed;
            QuizPanel.Visibility = Visibility.Visible;
            ShowQuestion();
        }

        private void BackToTopics_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RevisionTopicsPage());
        }
    }
}
