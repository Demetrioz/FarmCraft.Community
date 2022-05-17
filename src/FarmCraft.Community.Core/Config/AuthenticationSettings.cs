namespace FarmCraft.Community.Core.Config
{
    public class AuthenticationSettings
    {
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
        public string? SecretKey { get; set; }
        public int TokenDurationMinutes { get; set; }
    }
}
