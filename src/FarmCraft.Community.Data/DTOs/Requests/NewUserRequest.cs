using System.ComponentModel.DataAnnotations;

namespace FarmCraft.Community.Data.DTOs.Requests
{
    public class NewUserRequest
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public int RoleId { get; set; }
    }
}
