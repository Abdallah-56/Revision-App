using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Revision_App.Models
{
    [Table("QuizResults")]
    public class QuizResult : BaseModel
    {
        [PrimaryKey("QuizResultsID", false)]
        public long QuizResultsID { get; set; }

        [Column("UserID")]
        public long UserID { get; set; }

        [Column("QuestionID")]
        public long QuestionID { get; set; }

        [Column("UserAnswer")]
        public string? UserAnswer { get; set; }

        [Column("IsCorrect")]
        public bool? IsCorrect { get; set; }

        [Column("DateCompleted")]
        public DateTimeOffset DateCompleted { get; set; }

        [Column("AttemptID")]
        public Guid? AttemptID { get; set; }
    }
}
