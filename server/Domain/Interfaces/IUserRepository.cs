using Domain.Entities;

namespace Domain.Interfaces;

public interface IUserRepository
{
    Task<User> GetUserByIdAsync(string userId);
    Task<List<User>> SearchUsersAsync(string firstName, string lastName);
}