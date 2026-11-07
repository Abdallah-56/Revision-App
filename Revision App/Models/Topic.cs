using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Revision_App.Models
{
    [Table("Topics")]
    public class Topic : BaseModel
    {
        [PrimaryKey("TopicID")]
        public int TopicID { get; set; }

        [Column("TopicName")]
        public string TopicName { get; set; } = "";

        [Column("Description")]
        public string Description { get; set; } = "";
    }
}