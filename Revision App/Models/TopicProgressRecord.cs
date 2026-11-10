using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Revision_App.Models
{
    [Table("Progress")]
    public class TopicProgressRecord : BaseModel
    {
        [PrimaryKey("ProgressID", false)]
        public long ProgressID { get; set; }

        [Column("UserID")]
        public long UserID { get; set; }

        [Column("TopicID")]
        public int TopicID { get; set; }

        // This is the latest raw quiz score, not a value reduced repeatedly.
        [Column("ProgressPercentage")]
        public long? ProgressPercentage { get; set; }

        [Column("LastRevised")]
        public DateTime LastRevised { get; set; }
    }
}
