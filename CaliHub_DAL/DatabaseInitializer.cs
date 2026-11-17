using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace CaliHub_DAL
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(string baseConnectionString)
        {
            await CreateDatabaseAsync(baseConnectionString);

            string connectionStringWithDb = baseConnectionString + "Database=CaliHubDB;";

            await CreateTablesAsync(connectionStringWithDb);
            await SeedRolesAsync(connectionStringWithDb);
            await SeedUsersAsync(connectionStringWithDb);
        }

        private static async Task CreateDatabaseAsync(string connectionString)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string query = "CREATE DATABASE IF NOT EXISTS CaliHubDB;";
            using var command = new MySqlCommand(query, connection);
            await command.ExecuteNonQueryAsync();
        }

        private static async Task CreateTablesAsync(string connectionString)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            // 1. טבלת תפקידים
            string createRolesQuery = @"
                CREATE TABLE IF NOT EXISTS Roles (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    RoleName VARCHAR(50) NOT NULL UNIQUE
                );";
            using var commandRoles = new MySqlCommand(createRolesQuery, connection);
            await commandRoles.ExecuteNonQueryAsync();

            // 2. טבלת משתמשים - עם עמודת IsActive
            string createUsersQuery = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    Username VARCHAR(50) NOT NULL UNIQUE,
                    Email VARCHAR(100) NOT NULL UNIQUE,
                    PasswordHash VARCHAR(255) NOT NULL,
                    RoleId INT NOT NULL,
                    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
                    FOREIGN KEY (RoleId) REFERENCES Roles(Id)
                );";
            using var commandUsers = new MySqlCommand(createUsersQuery, connection);
            await commandUsers.ExecuteNonQueryAsync();
        }

        private static async Task SeedRolesAsync(string connectionString)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string checkQuery = "SELECT COUNT(*) FROM Roles;";
            using var checkCommand = new MySqlCommand(checkQuery, connection);
            long count = Convert.ToInt64(await checkCommand.ExecuteScalarAsync());

            if (count == 0)
            {
                // Id=1: Trainee, Id=2: Trainer, Id=3: SuperAdmin
                string insertQuery = @"
                    INSERT INTO Roles (RoleName) VALUES 
                    ('Trainee'), 
                    ('Trainer'), 
                    ('SuperAdmin');";
                using var insertCommand = new MySqlCommand(insertQuery, connection);
                await insertCommand.ExecuteNonQueryAsync();
            }
        }

        private static async Task SeedUsersAsync(string connectionString)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string checkQuery = "SELECT COUNT(*) FROM Users;";
            using var checkCommand = new MySqlCommand(checkQuery, connection);
            long count = Convert.ToInt64(await checkCommand.ExecuteScalarAsync());

            // אם הטבלה ריקה, נכניס 10 משתמשי בדיקה עם סיסמה מגובבת (123456)
            if (count == 0)
            {
                await InsertSeedUserAsync(connection, "admin", "admin@calihub.com", "123456", "SuperAdmin", true);
                await InsertSeedUserAsync(connection, "trainer_dan", "dan@calihub.com", "123456", "Trainer", true);
                await InsertSeedUserAsync(connection, "trainer_alex", "alex@calihub.com", "123456", "Trainer", true);
                await InsertSeedUserAsync(connection, "user_ron", "ron@calihub.com", "123456", "Trainee", true);
                await InsertSeedUserAsync(connection, "user_noam", "noam@calihub.com", "123456", "Trainee", true);
                await InsertSeedUserAsync(connection, "user_maya", "maya@calihub.com", "123456", "Trainee", true);
                await InsertSeedUserAsync(connection, "user_tom", "tom@calihub.com", "123456", "Trainee", true);
                await InsertSeedUserAsync(connection, "user_gal", "gal@calihub.com", "123456", "Trainee", true);
                await InsertSeedUserAsync(connection, "user_eli", "eli@calihub.com", "123456", "Trainee", true);
                await InsertSeedUserAsync(connection, "user_or", "or@calihub.com", "123456", "Trainee", false);
            }
        }

        private static async Task InsertSeedUserAsync(
            MySqlConnection connection,
            string username,
            string email,
            string plainPassword,
            string roleName,
            bool isActive)
        {
            string query = @"
                INSERT INTO Users (Username, Email, PasswordHash, RoleId, IsActive)
                VALUES (@username, @email, @passwordHash, (SELECT Id FROM Roles WHERE RoleName = @roleName), @isActive);";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@passwordHash", BCrypt.Net.BCrypt.HashPassword(plainPassword));
            cmd.Parameters.AddWithValue("@roleName", roleName);
            cmd.Parameters.AddWithValue("@isActive", isActive);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}