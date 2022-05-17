namespace FarmCraft.Community.Data.Messages.Authentication
{
    public class AskToCreateUser : IAuthenticationMessage
    {
        public string Username { get; private set; }
        public string Password { get; private set; }
        public int RoleId { get; private set; }

        public AskToCreateUser(string username, string password, int roleId)
        {
            Username = username;
            Password = password;
            RoleId = roleId;
        }
    }
}
