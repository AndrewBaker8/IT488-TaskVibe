using System.Collections.Generic;
using TaskVibe.UI.Models;

namespace TaskVibe.Core.Repositories
{
    public interface IUserRepository
    {
        IEnumerable<User> GetAllUsers();
        User GetUserById(int userId);
    }
}