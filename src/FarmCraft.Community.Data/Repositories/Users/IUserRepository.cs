﻿using FarmCraft.Community.Data.Entities.Users;

namespace FarmCraft.Community.Data.Repositories.Users
{
    public interface IUserRepository
    {
        Task<User?> FindUserById(Guid id);
        Task<User?> FindUserByName(string username);
        Task<User> CreateNewUser(User user);
        Task SetLastLogin(Guid userId, DateTimeOffset loginTime);
    }
}
