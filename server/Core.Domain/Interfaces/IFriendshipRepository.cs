using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DAO;

namespace Core.Domain.Interfaces;

public interface IFriendshipRepository
{
    Task AddAsync(string userId, string friendId);
    Task DeleteAsync(string userId, string friendId);
    Task<List<FriendDAO>> ListAsync(string userId);
}
