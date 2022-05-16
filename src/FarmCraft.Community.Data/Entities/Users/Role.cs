using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmCraft.Community.Data.Entities.Users
{
    [Table("role")]
    public class Role : FarmCraftBase
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("label")]
        public string Label { get; set; }
    }
}
