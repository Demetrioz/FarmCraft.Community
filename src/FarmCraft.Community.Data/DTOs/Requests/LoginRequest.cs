using System.ComponentModel.DataAnnotations;

namespace FarmCraft.Community.Data.DTOs.Requests
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
