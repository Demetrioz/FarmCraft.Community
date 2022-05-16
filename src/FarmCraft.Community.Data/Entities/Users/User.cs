using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmCraft.Community.Data.Entities.Users
{
    [Table("user")]
    public class User : FarmCraftBase
    {
        [Key]
        [Column("id")]
        public Guid  Id { get; set; }
        [Column("username")]
        public string Username { get; set; }
        [Column("password")]
        public string Password { get; set; }
        [Column("email")]
        public string? Email { get; set; }
        [Column("phone")]
        public string? Phone { get; set; }
        [Column("reset_required")]
        public bool ResetRequired { get; set; }
        [Column("last_login")]
        public DateTimeOffset LastLogin { get; set; }

        [Column("role_id")]
        public int RoleId { get; set; }
        [ForeignKey("RoleId")]
        public Role? Role { get; set; }
    }
}
