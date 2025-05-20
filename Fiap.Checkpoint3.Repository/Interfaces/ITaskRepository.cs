using Fiap.Checkpoint3.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fiap.Checkpoint3.Repository.Interfaces
{
    public interface ITaskRepository
    {
        Task<IEnumerable<TaskItem>> GetAllByUserAsync(int userId);
        Task<TaskItem?> GetByIdAsync(int id, int userId);
        Task CreateAsync(TaskItem task);
        Task UpdateAsync(TaskItem task);
        Task DeleteAsync(int id, int userId);
        Task<bool> ExistsWithTitleOnDateAsync(string title, int userId, DateTime date);
    }
}
