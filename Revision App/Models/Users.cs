using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Revision_App.Models
{
    [Table("Users")]
    public class User : BaseModel
    {
        [PrimaryKey("UserID", false)]
        public long UserID { get; set; }

        [Column("FirstName")]
        public string FirstName { get; set; }

        [Column("LastName")]
        public string LastName { get; set; }

        [Column("Email")]
        public string Email { get; set; }

        [Column("XP")]
        public long XP { get; set; }

        [Column("AuthUserID")]
        public string AuthUserID { get; set; }
    }
}