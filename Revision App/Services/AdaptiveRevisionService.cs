using System;
using System.Collections.Generic;
using System.Linq;

namespace Revision_App.Services
{
    public class RevisionRecommendation
    {
        public int TopicID { get; set; }
        public string TopicName { get; set; } = "";
        public int PriorityScore { get; set; }
        public string PriorityLabel { get; set; } = "";
        public string RetentionLabel { get; set; } = "";
        public string Reason { get; set; } = "";
    }

    public class AdaptiveRevisionService
    {
        // Rank topics using the latest quiz score and knowledge decay.
        // A lower score means greater need for revision. Unstudied topics
        // receive a starting priority but are not assigned a false quiz score.
        public List<RevisionRecommendation> GetRecommendations(
            ProgressSnapshot progress, int maximum = 3)
        {
            if (progress == null)
                throw new ArgumentNullException(nameof(progress));

            if (maximum <= 0)
                return new List<RevisionRecommendation>();

            return progress.Topics
                .Select(topic =>
                {
                    if (!topic.HasStudied)
                    {
                        return new RevisionRecommendation
                        {
                            TopicID = topic.TopicID,
                            TopicName = topic.TopicName,
                            PriorityScore = 65,
                            PriorityLabel = "Not studied yet",
                            RetentionLabel = "No quiz score recorded",
                            Reason = "Start this topic to establish your first score."
                        };
                    }

                    int rawScore = Math.Clamp(topic.LatestScore, 0, 100);
                    int retention = Math.Clamp(topic.Percentage, 0, 100);
                    int days = Math.Max(0, topic.DaysSinceRevision);

                    // Priority = 70% estimated knowledge gap + 30% recent
                    // assessment gap. Decay already includes time since revision.
                    int priority = (int)Math.Round(
                        (100 - retention) * 0.70 +
                        (100 - rawScore) * 0.30,
                        MidpointRounding.AwayFromZero);

                    string reason;
                    if (rawScore < 60)
                        reason = $"Latest score was {rawScore}%. Practise the weaker content.";
                    else if (days > 0)
                        reason = $"Last revised {days} day(s) ago. Revisit to maintain retention.";
                    else
                        reason = "Revisit this topic to reinforce what you learned.";

                    return new RevisionRecommendation
                    {
                        TopicID = topic.TopicID,
                        TopicName = topic.TopicName,
                        PriorityScore = priority,
                        PriorityLabel = $"Priority: {priority}/100",
                        RetentionLabel = $"Estimated retention: {retention}%",
                        Reason = reason
                    };
                })
                .OrderByDescending(r => r.PriorityScore)
                .ThenBy(r => r.TopicName, StringComparer.OrdinalIgnoreCase)
                .Take(maximum)
                .ToList();
        }
    }
}
