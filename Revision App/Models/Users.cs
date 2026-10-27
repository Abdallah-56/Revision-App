using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Revision_App.Models
{
    [Table("Users")]
    public class User : BaseModel
    {
        [PrimaryKey("UserID", false)]
        public long UserID { get; set; }

        [Column("AuthUserID")]
        public string AuthUserID { get; set; } = string.Empty;

        [Column("FirstName")]
        public string FirstName { get; set; } = string.Empty;

        [Column("LastName")]
        public string LastName { get; set; } = string.Empty;

        [Column("Email")]
        public string Email { get; set; } = string.Empty;

        [Column("XP")]
        public long XP { get; set; }
    }
}