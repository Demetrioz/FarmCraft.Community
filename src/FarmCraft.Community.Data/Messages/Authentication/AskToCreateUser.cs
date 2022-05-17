namespace FarmCraft.Community.Data.Messages.Authentication
{
    public class AskToCreateUser : IAuthenticationMessage
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; }

        public AskToCreateUser(string username, string password, int roleId)
        {
            Username = username;
            Password = password;
            RoleId = roleId;
        }
    }
}
