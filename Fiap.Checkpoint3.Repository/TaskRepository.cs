using Fiap.Checkpoint3.Model;
using Fiap.Checkpoint3.Repository.Interfaces;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fiap.Checkpoint3.Repository
{
    public class TaskRepository : ITaskRepository
    {
        private readonly string _connectionString;
        public TaskRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<TaskItem>> GetAllByUserAsync(int userId)
        {
            var list = new List<TaskItem>();
            const string sql = @"
                SELECT id, title, description, priority, status, created_at, user_id
                  FROM TAREFASPRO_TAREFA
                 WHERE user_id = :userId
              ORDER BY created_at DESC";

            await using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new OracleCommand(sql, conn) { BindByName = true };
            cmd.Parameters.Add("userId", OracleDbType.Int32).Value = userId;

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new TaskItem
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Priority = reader.GetString(3),
                    Status = reader.GetString(4),
                    CreatedAt = reader.GetDateTime(5),
                    UserId = reader.GetInt32(6)
                });
            }
            return list;
        }

        public async Task<TaskItem?> GetByIdAsync(int id, int userId)
        {
            const string sql = @"
                SELECT id, title, description, priority, status, created_at, user_id
                  FROM TAREFASPRO_TAREFA
                 WHERE id = :id AND user_id = :userId";

            await using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new OracleCommand(sql, conn) { BindByName = true };
            cmd.Parameters.Add("id", OracleDbType.Int32).Value = id;
            cmd.Parameters.Add("userId", OracleDbType.Int32).Value = userId;

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;
            return new TaskItem
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                Priority = reader.GetString(3),
                Status = reader.GetString(4),
                CreatedAt = reader.GetDateTime(5),
                UserId = reader.GetInt32(6)
            };
        }

        public async Task CreateAsync(TaskItem task)
        {
            const string sql = @"
                INSERT INTO TAREFASPRO_TAREFA (
                    title,
                    description,
                    priority,
                    status,
                    created_at,
                    user_id
                ) VALUES (
                    :title,
                    :taskDescription,
                    :prio,
                    :stat,
                    SYSDATE,
                    :userId
                )";

            await using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new OracleCommand(sql, conn)
            {
                BindByName = true
            };
            cmd.Parameters.Add("title", OracleDbType.Varchar2).Value = task.Title;
            cmd.Parameters.Add("taskDescription", OracleDbType.Varchar2).Value = task.Description ?? "";
            cmd.Parameters.Add("prio", OracleDbType.Varchar2).Value = task.Priority;
            cmd.Parameters.Add("stat", OracleDbType.Varchar2).Value = task.Status;
            cmd.Parameters.Add("userId", OracleDbType.Int32).Value = task.UserId;

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateAsync(TaskItem task)
        {
            const string sql = @"
                UPDATE TAREFASPRO_TAREFA
                   SET title       = :title,
                       description = :taskDescription,
                       priority    = :prio,
                       status      = :stat
                 WHERE id = :id
                   AND user_id = :userId";

            await using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new OracleCommand(sql, conn)
            {
                BindByName = true
            };
            cmd.Parameters.Add("title", OracleDbType.Varchar2).Value = task.Title;
            cmd.Parameters.Add("taskDescription", OracleDbType.Varchar2).Value = task.Description ?? "";
            cmd.Parameters.Add("prio", OracleDbType.Varchar2).Value = task.Priority;
            cmd.Parameters.Add("stat", OracleDbType.Varchar2).Value = task.Status;
            cmd.Parameters.Add("id", OracleDbType.Int32).Value = task.Id;
            cmd.Parameters.Add("userId", OracleDbType.Int32).Value = task.UserId;

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(int id, int userId)
        {
            const string sql = @"
                DELETE FROM TAREFASPRO_TAREFA
                 WHERE id = :id AND user_id = :userId";

            await using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new OracleCommand(sql, conn) { BindByName = true };
            cmd.Parameters.Add("id", OracleDbType.Int32).Value = id;
            cmd.Parameters.Add("userId", OracleDbType.Int32).Value = userId;

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> ExistsWithTitleOnDateAsync(string title, int userId, DateTime date)
        {
            const string sql = @"
                SELECT COUNT(1)
                  FROM TAREFASPRO_TAREFA
                 WHERE user_id            = :userId
                   AND TRUNC(created_at)  = TRUNC(:targetDate)
                   AND UPPER(title)       = UPPER(:title)";

            await using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new OracleCommand(sql, conn)
            {
                BindByName = true    // essencial para casar pelo nome dos binds
            };

            cmd.Parameters.Add("userId", OracleDbType.Int32).Value = userId;
            cmd.Parameters.Add("targetDate", OracleDbType.Date).Value = date;
            cmd.Parameters.Add("title", OracleDbType.Varchar2).Value = title;

            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return count > 0;
        }

    }
}