using Fiap.Checkpoint3.Model;
using Fiap.Checkpoint3.Repository.Interfaces;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;

namespace Fiap.Checkpoint3.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            // aplica o hash para comparar
            string passwordHash = ComputeHash(password);

            const string sql = @"
                SELECT COUNT(1)
                  FROM TAREFASPRO_USER
                 WHERE username      = :username
                   AND password_hash = :password";

            await using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new OracleCommand(sql, conn);
            cmd.Parameters.Add("username", OracleDbType.Varchar2).Value = username;
            cmd.Parameters.Add("password", OracleDbType.Varchar2).Value = passwordHash;

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            const string sql = @"
                SELECT id, username, password_hash, full_name, email, role, created_at
                  FROM TAREFASPRO_USER
                 WHERE username = :username";

            await using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new OracleCommand(sql, conn);
            cmd.Parameters.Add("username", OracleDbType.Varchar2).Value = username;

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            return new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                FullName = reader.GetString(3),
                Email = reader.GetString(4),
                Role = reader.GetString(5),
                CreatedAt = reader.GetDateTime(6)
            };
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            const string sql = @"
                SELECT id, username, password_hash, full_name, email, role, created_at
                  FROM TAREFASPRO_USER";

            var users = new List<User>();
            await using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new OracleCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                users.Add(new User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    PasswordHash = reader.GetString(2),
                    FullName = reader.GetString(3),
                    Email = reader.GetString(4),
                    Role = reader.GetString(5),
                    CreatedAt = reader.GetDateTime(6)
                });
            }

            return users;
        }

        public async Task CreateAsync(User user)
        {
            const string sql = @"
                INSERT INTO TAREFASPRO_USER (username, password_hash, full_name, email, role, created_at)
                VALUES (:username, :password, :fullName, :email, :role, SYSDATE)";

            // gera hash da senha antes de inserir
            string pwHash = ComputeHash(user.PasswordHash);

            await using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new OracleCommand(sql, conn);
            cmd.Parameters.Add("username", OracleDbType.Varchar2).Value = user.Username;
            cmd.Parameters.Add("password", OracleDbType.Varchar2).Value = pwHash;
            cmd.Parameters.Add("fullName", OracleDbType.Varchar2).Value = user.FullName;
            cmd.Parameters.Add("email", OracleDbType.Varchar2).Value = user.Email;
            cmd.Parameters.Add("role", OracleDbType.Varchar2).Value = user.Role;

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateAsync(User user)
        {
            const string sql = @"
                UPDATE TAREFASPRO_USER
                   SET full_name     = :fullName,
                       email         = :email,
                       role          = :role
                 WHERE id = :id";

            await using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new OracleCommand(sql, conn);
            cmd.Parameters.Add("fullName", OracleDbType.Varchar2).Value = user.FullName;
            cmd.Parameters.Add("email", OracleDbType.Varchar2).Value = user.Email;
            cmd.Parameters.Add("role", OracleDbType.Varchar2).Value = user.Role;
            cmd.Parameters.Add("id", OracleDbType.Int32).Value = user.Id;

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(int id)
        {
            const string sql = @"DELETE FROM TAREFASPRO_USER WHERE id = :id";

            await using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new OracleCommand(sql, conn);
            cmd.Parameters.Add("id", OracleDbType.Int32).Value = id;

            await cmd.ExecuteNonQueryAsync();
        }

        private string ComputeHash(string input)
        {
            // SHA256 + Base64
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
