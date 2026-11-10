using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Revision_App.Services
{
    public class QuizAnswer
    {
        public long QuestionID { get; set; }
        public string SelectedOption { get; set; } = "";
    }

    public class QuizAttemptService
    {
        public async Task SaveAttemptAsync(
            Guid attemptID, int topicID, List<QuizAnswer> answers)
        {
            var client = SupabaseService.Client
                ?? throw new InvalidOperationException("Supabase is not initialised.");

            if (client.Auth.CurrentUser == null)
                throw new InvalidOperationException("Please sign in again.");

            if (answers.Count == 0)
                throw new InvalidOperationException("There are no quiz answers to save.");

            await client.Rpc("save_quiz_attempt", new
            {
                p_attempt_id = attemptID.ToString(),
                p_topic_id = topicID,
                p_answers = answers.Select(a => new
                {
                    question_id = a.QuestionID,
                    answer = a.SelectedOption
                }).ToArray()
            });
        }
    }
}
