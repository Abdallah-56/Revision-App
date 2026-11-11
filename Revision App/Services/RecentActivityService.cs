using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Revision_App.Models;

namespace Revision_App.Services
{
    public class RecentActivityItem
    {
        public string TopicName { get; set; } = "";
        public string ResultLabel { get; set; } = "";
        public string DateLabel { get; set; } = "";
        public DateTimeOffset CompletedAt { get; set; }
    }

    public class RecentActivityService
    {
        public async Task<List<RecentActivityItem>> GetRecentActivityAsync(int maximum = 4)
        {
            if (maximum <= 0)
                return new List<RecentActivityItem>();

            var client = SupabaseService.Client
                ?? throw new InvalidOperationException("Supabase is not initialised.");
            var auth = client.Auth.CurrentUser
                ?? throw new InvalidOperationException("Please sign in again.");
            var users = await client.From<User>()
                .Where(u => u.AuthUserID == auth.Id).Get();
            var user = users.Models.FirstOrDefault()
                ?? throw new InvalidOperationException("Profile not found.");

            var resultsResponse = await client.From<QuizResult>()
                .Where(r => r.UserID == user.UserID).Get();
            var questionsResponse = await client.From<Question>().Get();
            var topicsResponse = await client.From<Topic>().Get();

            var questionTopic = questionsResponse.Models
                .ToDictionary(q => q.QuestionID, q => q.TopicID);
            var topicNames = topicsResponse.Models
                .ToDictionary(t => t.TopicID, t => t.TopicName);

            return resultsResponse.Models
                .Where(r => r.AttemptID.HasValue && questionTopic.ContainsKey(r.QuestionID))
                .GroupBy(r => r.AttemptID!.Value)
                .Select(group =>
                {
                    var rows = group.ToList();
                    int topicId = questionTopic[rows[0].QuestionID];
                    int correct = rows.Count(r => r.IsCorrect == true);
                    int total = rows.Count;
                    int percent = (int)Math.Round(100.0 * correct / total,
                        MidpointRounding.AwayFromZero);
                    var completed = rows.Max(r => r.DateCompleted);
                    return new RecentActivityItem
                    {
                        TopicName = topicNames.TryGetValue(topicId, out var name)
                            ? name : "Revision quiz",
                        ResultLabel = $"{correct}/{total} correct  ·  {percent}%",
                        DateLabel = completed.ToLocalTime().ToString("dd MMM yyyy, HH:mm"),
                        CompletedAt = completed
                    };
                })
                .OrderByDescending(item => item.CompletedAt)
                .Take(maximum)
                .ToList();
        }
    }
}
