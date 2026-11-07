using Revision_App.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Revision_App.Services
{
    public class QuestionService
    {
        public async Task<List<Topic>> GetTopicsAsync()
        {
            var response = await SupabaseService.Client
                .From<Topic>()
                .Get();

            return response.Models.ToList();
        }

        public async Task<List<Question>> GetQuestionsAsync(int topicID)
        {
            var response = await SupabaseService.Client
                .From<Question>()
                .Where(q => q.TopicID == topicID)
                .Get();

            return response.Models.ToList();
        }
    }
}