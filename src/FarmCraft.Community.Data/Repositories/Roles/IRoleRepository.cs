using FarmCraft.Community.Data.Entities.Users;

namespace FarmCraft.Community.Data.Repositories.Roles
{
    public interface IRoleRepository
    {
        Task<List<Role>> GetRoles();
        Task<Role?> GetRoleByName(string name);
        Task<Role?> GetRoleById(int id);
    }
}
