using System;

namespace Revision_App.Services
{
    public static class KnowledgeDecayCalculator
    {
        // Retain 98% of the previous day's estimated knowledge.
        private const double DailyRetention = 0.98;

        public static int Calculate(long latestScore, DateTime lastRevisedUtc, DateTime todayUtc)
        {
            long startingScore = Math.Clamp(latestScore, 0L, 100L);
            int days = DaysSinceRevision(lastRevisedUtc, todayUtc);
            double estimate = startingScore * Math.Pow(DailyRetention, days);
            return (int)Math.Round(estimate, MidpointRounding.AwayFromZero);
        }

        public static int DaysSinceRevision(DateTime lastRevisedUtc, DateTime todayUtc)
        {
            return Math.Max(0, (int)(todayUtc.Date - lastRevisedUtc.Date).TotalDays);
        }
    }
}
