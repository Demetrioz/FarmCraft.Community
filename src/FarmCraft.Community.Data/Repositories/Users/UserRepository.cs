using FarmCraft.Community.Data.Context;
using FarmCraft.Community.Data.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace FarmCraft.Community.Data.Repositories.Users
{
    public class UserRepository : FarmCraftRepository, IUserRepository
    {
        public UserRepository(FarmCraftContext dbContext) : base(dbContext)
        {
        }

        public async Task<User?> FindUserById(Guid id)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .Where(u => u.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<User?> FindUserByName(string username)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .Where(u => u.Username == username)
                .FirstOrDefaultAsync();
        }

        public async Task<User> CreateNewUser(User user)
        {
            User addedUser = (await _dbContext
                .AddAsync(user)).Entity;
            await _dbContext.SaveChangesAsync();

            return addedUser;
        }

        public async Task SetLastLogin(Guid userId, DateTimeOffset loginTime)
        {
            User? user = await _dbContext.Users
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync();

            if (user == null)
                throw new ArgumentException($"Cannot find user {userId}");

            user.LastLogin = loginTime.UtcDateTime;
            await _dbContext.SaveChangesAsync();
        }
    }
}
