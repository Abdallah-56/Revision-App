using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Revision_App.Models;

namespace Revision_App.Services
{
    public class TopicProgressItem
    {
        public string TopicName { get; set; } = "";
        // The progress bar shows estimated retention, not a modified quiz score.
        public int Percentage { get; set; }
        public string ScoreComparison { get; set; } = "";
        public string LastRevised { get; set; } = "Not studied yet";
    }

    public class QuizHistoryItem
    {
        public string DateLabel { get; set; } = "";
        public string PercentageLabel { get; set; } = "";
        public string TopicLabel { get; set; } = "";
        public double BarHeight { get; set; }
    }

    public class ProgressSnapshot
    {
        public int OverallProgress { get; set; }
        public long XP { get; set; }
        public int DayStreak { get; set; }
        public int QuizzesCompleted { get; set; }
        public int TopicsStudied { get; set; }
        public int TotalTopics { get; set; }
        public int AverageScore { get; set; }
        public List<TopicProgressItem> Topics { get; set; } = new();
        public List<QuizHistoryItem> History { get; set; } = new();
    }

    public class ProgressService
    {
        private class QuizAttempt
        {
            public int TopicID { get; set; }
            public DateTimeOffset CompletedAt { get; set; }
            public int Correct { get; set; }
            public int Total { get; set; }
            public int Percentage => Total == 0 ? 0 :
                (int)Math.Round(Correct * 100.0 / Total, MidpointRounding.AwayFromZero);
        }

        public async Task<ProgressSnapshot> GetProgressAsync()
        {
            var client = SupabaseService.Client
                ?? throw new InvalidOperationException("Supabase is not initialised.");
            var signedInUser = client.Auth.CurrentUser
                ?? throw new InvalidOperationException("Please sign in again.");

            var userResponse = await client.From<User>()
                .Where(u => u.AuthUserID == signedInUser.Id)
                .Get();
            var user = userResponse.Models.FirstOrDefault()
                ?? throw new InvalidOperationException("User profile was not found.");

            var topicsResponse = await client.From<Topic>().Get();
            var questionsResponse = await client.From<Question>().Get();
            var resultsResponse = await client.From<QuizResult>()
                .Where(r => r.UserID == user.UserID)
                .Get();
            var progressResponse = await client.From<TopicProgressRecord>()
                .Where(p => p.UserID == user.UserID)
                .Get();

            var topics = topicsResponse.Models.ToList();
            var questionTopics = questionsResponse.Models
                .ToDictionary(q => q.QuestionID, q => q.TopicID);
            var results = resultsResponse.Models.ToList();
            var storedProgress = progressResponse.Models
                .Where(p => p.TopicID != 0 && p.ProgressPercentage.HasValue)
                .GroupBy(p => p.TopicID)
                .ToDictionary(group => group.Key,
                    group => group.OrderByDescending(p => p.LastRevised).First());

            // Each quiz attempt has one AttemptID shared by all its answers.
            var attempts = results
                .Where(r => r.AttemptID.HasValue && questionTopics.ContainsKey(r.QuestionID))
                .GroupBy(r => r.AttemptID!.Value)
                .Select(group =>
                {
                    var rows = group.OrderBy(r => r.DateCompleted).ToList();
                    return new QuizAttempt
                    {
                        TopicID = questionTopics[rows[0].QuestionID],
                        CompletedAt = rows.Max(r => r.DateCompleted),
                        Correct = rows.Count(r => r.IsCorrect == true),
                        Total = rows.Count
                    };
                })
                .ToList();

            var snapshot = new ProgressSnapshot
            {
                XP = user.XP,
                TotalTopics = topics.Count,
                QuizzesCompleted = attempts.Count,
                TopicsStudied = attempts.Select(a => a.TopicID).Distinct().Count(),
                AverageScore = attempts.Count == 0 ? 0 :
                    (int)Math.Round(attempts.Sum(a => a.Correct) * 100.0 /
                                    attempts.Sum(a => a.Total), MidpointRounding.AwayFromZero)
            };

            DateTime todayUtc = DateTime.UtcNow.Date;
            foreach (var topic in topics.OrderBy(t => t.TopicName))
            {
                var latest = attempts
                    .Where(a => a.TopicID == topic.TopicID)
                    .OrderByDescending(a => a.CompletedAt)
                    .FirstOrDefault();

                int latestScore;
                DateTime lastRevisedDate;
                if (storedProgress.TryGetValue(topic.TopicID, out var saved))
                {
                    latestScore = (int)Math.Clamp(saved.ProgressPercentage!.Value, 0L, 100L);
                    lastRevisedDate = saved.LastRevised.Date;
                }
                else if (latest != null)
                {
                    // Fallback for older quiz attempts that predate the Progress table.
                    latestScore = latest.Percentage;
                    lastRevisedDate = latest.CompletedAt.UtcDateTime.Date;
                }
                else
                {
                    snapshot.Topics.Add(new TopicProgressItem
                    {
                        TopicName = topic.TopicName,
                        Percentage = 0,
                        ScoreComparison = "Complete a quiz to begin tracking this topic.",
                        LastRevised = "Not studied yet"
                    });
                    continue;
                }

                int days = KnowledgeDecayCalculator.DaysSinceRevision(lastRevisedDate, todayUtc);
                int retained = KnowledgeDecayCalculator.Calculate(latestScore, lastRevisedDate, todayUtc);
                snapshot.Topics.Add(new TopicProgressItem
                {
                    TopicName = topic.TopicName,
                    Percentage = retained,
                    ScoreComparison = $"Latest quiz: {latestScore}%  ·  Estimated retention: {retained}%",
                    LastRevised = $"Last revised: {lastRevisedDate:dd MMM yyyy} (UTC)  ·  {days} day(s) ago"
                });
            }

            // Unstudied topics count as 0; studied topics show estimated retention.
            snapshot.OverallProgress = topics.Count == 0 ? 0 :
                (int)Math.Round(snapshot.Topics.Sum(t => t.Percentage) /
                                (double)topics.Count, MidpointRounding.AwayFromZero);

            snapshot.DayStreak = CalculateStreak(attempts.Select(a => a.CompletedAt.ToLocalTime().Date));

            // The graph always shows original quiz scores, without decay.
            var topicNames = topics.ToDictionary(t => t.TopicID, t => t.TopicName);
            snapshot.History = attempts
                .OrderByDescending(a => a.CompletedAt)
                .Take(7)
                .OrderBy(a => a.CompletedAt)
                .Select(a => new QuizHistoryItem
                {
                    DateLabel = a.CompletedAt.ToLocalTime().ToString("dd MMM"),
                    PercentageLabel = a.Percentage + "%",
                    TopicLabel = topicNames.TryGetValue(a.TopicID, out var name) ? name : "Quiz",
                    BarHeight = Math.Max(3, a.Percentage * 1.35)
                })
                .ToList();

            return snapshot;
        }

        private static int CalculateStreak(IEnumerable<DateTime> completedDays)
        {
            var days = new HashSet<DateTime>(completedDays.Select(d => d.Date));
            var date = DateTime.Today;
            if (!days.Contains(date))
                date = date.AddDays(-1);

            int streak = 0;
            while (days.Contains(date))
            {
                streak++;
                date = date.AddDays(-1);
            }
            return streak;
        }
    }
}
