using FarmCraft.Community.Data.Context;

namespace FarmCraft.Community.Data.Repositories
{
    public abstract class FarmCraftRepository
    {
        protected readonly FarmCraftContext _dbContext;

        public FarmCraftRepository(FarmCraftContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
