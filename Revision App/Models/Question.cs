using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Revision_App.Models
{
    [Table("Questions")]
    public class Question : BaseModel
    {
        [PrimaryKey("QuestionID")]
        public long QuestionID { get; set; }

        [Column("TopicID")]
        public int TopicID { get; set; }

        [Column("QuestionText")]
        public string QuestionText { get; set; } = "";

        [Column("QuestionType")]
        public string QuestionType { get; set; } = "";

        [Column("CorrectAnswer")]
        public string CorrectAnswer { get; set; } = "";

        [Column("Difficulty")]
        public short Difficulty { get; set; }

        [Column("OptionA")]
        public string OptionA { get; set; } = "";

        [Column("OptionB")]
        public string OptionB { get; set; } = "";

        [Column("OptionC")]
        public string OptionC { get; set; } = "";

        [Column("OptionD")]
        public string OptionD { get; set; } = "";

        [Column("CorrectOption")]
        public string CorrectOption { get; set; } = "";

        [Column("Explanation")]
        public string Explanation { get; set; } = "";
    }
}