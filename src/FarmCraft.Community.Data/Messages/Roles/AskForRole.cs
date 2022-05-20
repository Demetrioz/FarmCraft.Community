namespace FarmCraft.Community.Data.Messages.Roles
{
    public class AskForRole : IRoleMessage
    {
        public string? Name { get; private set; }
        public int? Id { get; private set; }

        public AskForRole(int? id, string? name)
        {
            Id = id;
            Name = name;
        }
        //public AskForRole(string name)
        //{
        //    Name = name;
        //}

        //public AskForRole(int id)
        //{
        //    Id = id;
        //}
    }
}
