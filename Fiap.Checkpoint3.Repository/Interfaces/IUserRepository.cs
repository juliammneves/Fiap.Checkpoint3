using Fiap.Checkpoint3.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fiap.Checkpoint3.Repository.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> ValidateUserAsync(string username, string password);
        Task<User?> GetByUsernameAsync(string username);
        Task<IEnumerable<User>> GetAllAsync();
        Task CreateAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(int id);
    }
}
