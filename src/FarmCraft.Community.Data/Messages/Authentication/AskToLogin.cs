namespace FarmCraft.Community.Data.Messages.Authentication
{
    public class AskToLogin : IAuthenticationMessage
    {
        public string Username { get; private set; }
        public string Password { get; private set; }

        public AskToLogin(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}
