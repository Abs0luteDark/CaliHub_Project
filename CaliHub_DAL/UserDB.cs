using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using CaliHub_Model;

namespace CaliHub_DAL
{
    public class UserDB
    {
        private readonly string _connectionString;

        public UserDB(string connectionString)
        {
            _connectionString = connectionString;
        }

        private User ReadUser(MySqlDataReader reader)
        {
            int id = reader.GetInt32("Id");
            string username = reader.GetString("Username");
            string email = reader.GetString("Email");
            string passwordHash = reader.GetString("PasswordHash");
            int roleId = reader.GetInt32("RoleId");
            bool isActive = reader.GetBoolean("IsActive");

            string roleName = "";
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals("RoleName", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                {
                    roleName = reader.GetString(i);
                    break;
                }
            }

            return new User(id, username, email, passwordHash, roleId, isActive, roleName);
        }

        public async Task<int> AddUserAsync(User user)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"INSERT INTO Users (Username, Email, PasswordHash, RoleId, IsActive)
                             VALUES (@username, @email, @password_hash, @role_id, @is_active);
                             SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@username", user.Username);
            cmd.Parameters.AddWithValue("@email", user.Email);
            cmd.Parameters.AddWithValue("@password_hash", user.PasswordHash);
            cmd.Parameters.AddWithValue("@role_id", user.RoleId);
            cmd.Parameters.AddWithValue("@is_active", user.IsActive);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = "SELECT COUNT(1) FROM Users WHERE Username = @username;";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@username", username);

            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return count > 0;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = "SELECT COUNT(1) FROM Users WHERE Email = @email;";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@email", email);

            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return count > 0;
        }

        public async Task<User?> CheckLoginAsync(string username, string password)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"SELECT u.Id, u.Username, u.Email, u.PasswordHash, u.RoleId, u.IsActive, r.RoleName
                             FROM Users u
                             JOIN Roles r ON u.RoleId = r.Id
                             WHERE u.Username = @username AND u.IsActive = 1;";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@username", username);

            using var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var user = ReadUser(reader);
                if (user.PasswordHash == password)
                {
                    return user;
                }
            }

            return null;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"SELECT u.Id, u.Username, u.Email, u.PasswordHash, u.RoleId, u.IsActive, r.RoleName
                             FROM Users u
                             JOIN Roles r ON u.RoleId = r.Id
                             WHERE u.Id = @id;";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return ReadUser(reader);
            }

            return null;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"SELECT u.Id, u.Username, u.Email, u.PasswordHash, u.RoleId, u.IsActive, r.RoleName
                             FROM Users u
                             JOIN Roles r ON u.RoleId = r.Id;";

            using var cmd = new MySqlCommand(query, conn);
            using var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                users.Add(ReadUser(reader));
            }

            return users;
        }

        public async Task<Dictionary<string, int>> CountUsersByRoleAsync()
        {
            var result = new Dictionary<string, int>();
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"SELECT r.RoleName, COUNT(u.Id) AS UserCount
                             FROM Roles r
                             LEFT JOIN Users u ON r.Id = u.RoleId
                             GROUP BY r.RoleName;";

            using var cmd = new MySqlCommand(query, conn);
            using var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                string roleName = reader.GetString("RoleName");
                int count = reader.GetInt32("UserCount");
                result[roleName] = count;
            }

            return result;
        }

        public async Task<bool> UpdateUserRoleAsync(int userId, int newRoleId)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = "UPDATE Users SET RoleId = @role_id WHERE Id = @id;";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@role_id", newRoleId);
            cmd.Parameters.AddWithValue("@id", userId);

            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string newPasswordHash)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = "UPDATE Users SET PasswordHash = @password_hash WHERE Id = @id;";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@password_hash", newPasswordHash);
            cmd.Parameters.AddWithValue("@id", userId);

            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateEmailAsync(int userId, string newEmail)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = "UPDATE Users SET Email = @email WHERE Id = @id;";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@email", newEmail);
            cmd.Parameters.AddWithValue("@id", userId);

            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
    }
}