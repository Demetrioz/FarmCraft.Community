using FarmCraft.Community.Data.Context;
using FarmCraft.Community.Data.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace FarmCraft.Community.Data.Repositories.Roles
{
    public class RoleRepository : FarmCraftRepository, IRoleRepository
    {
        public RoleRepository(FarmCraftContext dbContext) : base(dbContext)
        {
        }

        public async Task<Role?> GetRoleById(int id)
        {
            return await _dbContext.Roles
                .AsNoTracking()
                .Where(r => r.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Role?> GetRoleByName(string name)
        {
            return await _dbContext.Roles
                .AsNoTracking()
                .Where(r => r.Name == name)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Role>> GetRoles()
        {
            return await _dbContext.Roles
                .ToListAsync();
        }
    }
}
