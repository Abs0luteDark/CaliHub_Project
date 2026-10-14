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
            await SeedUsersAsync(connectionStringWithDb); // הוספנו אתחול משתמשים
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

            // אם הטבלה ריקה, נכניס 10 משתמשי בדיקה (1 מנהל, 2 מאמנים, 7 מתאמנים)
            if (count == 0)
            {
                string insertQuery = @"
                    INSERT INTO Users (Username, Email, PasswordHash, RoleId, IsActive) VALUES 
                    ('admin', 'admin@calihub.com', '123456', 3, TRUE),
                    ('trainer_dan', 'dan@calihub.com', '123456', 2, TRUE),
                    ('trainer_alex', 'alex@calihub.com', '123456', 2, TRUE),
                    ('user_ron', 'ron@calihub.com', '123456', 1, TRUE),
                    ('user_noam', 'noam@calihub.com', '123456', 1, TRUE),
                    ('user_maya', 'maya@calihub.com', '123456', 1, TRUE),
                    ('user_tom', 'tom@calihub.com', '123456', 1, TRUE),
                    ('user_gal', 'gal@calihub.com', '123456', 1, TRUE),
                    ('user_eli', 'eli@calihub.com', '123456', 1, TRUE),
                    ('user_or', 'or@calihub.com', '123456', 1, FALSE);"; // משתמש אחד לא פעיל לצורכי בדיקה

                using var insertCommand = new MySqlCommand(insertQuery, connection);
                await insertCommand.ExecuteNonQueryAsync();
            }
        }
    }
}