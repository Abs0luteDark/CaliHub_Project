using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace CaliHub_DAL
{
    public static class DatabaseInitializer
    {
        // הפעולה הראשית שמופעלת מה-API
        public static async Task InitializeAsync(string baseConnectionString)
        {
            // 1. יצירת בסיס הנתונים (אם לא קיים)
            await CreateDatabaseAsync(baseConnectionString);

            // 2. הוספת שם בסיס הנתונים למחרוזת החיבור כדי שנוכל ליצור בו טבלאות
            string connectionStringWithDb = baseConnectionString + "Database=CaliHubDB;";

            // 3. יצירת הטבלאות (בסדר הנכון)
            await CreateTablesAsync(connectionStringWithDb);

            // 4. אתחול טבלת התפקידים עם התפקידים ממסמך הייזום
            await SeedRolesAsync(connectionStringWithDb);
        }

        private static async Task CreateDatabaseAsync(string connectionString)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            // יצירת מסד נתונים לפרויקט CaliHub
            string query = "CREATE DATABASE IF NOT EXISTS CaliHubDB;";
            using var command = new MySqlCommand(query, connection);
            await command.ExecuteNonQueryAsync();
        }

        private static async Task CreateTablesAsync(string connectionString)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            // יצירת טבלת תפקידים (חייבת להיות ראשונה כי משתמשים מצביעים עליה)
            string createRolesQuery = @"
                CREATE TABLE IF NOT EXISTS Roles (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    RoleName VARCHAR(50) NOT NULL UNIQUE
                );";
            using var commandRoles = new MySqlCommand(createRolesQuery, connection);
            await commandRoles.ExecuteNonQueryAsync();

            // יצירת טבלת משתמשים עם מפתח זר שמצביע לטבלת התפקידים
            string createUsersQuery = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    Username VARCHAR(50) NOT NULL UNIQUE,
                    Email VARCHAR(100) NOT NULL UNIQUE,
                    PasswordHash VARCHAR(255) NOT NULL,
                    RoleId INT NOT NULL,
                    FOREIGN KEY (RoleId) REFERENCES Roles(Id)
                );";
            using var commandUsers = new MySqlCommand(createUsersQuery, connection);
            await commandUsers.ExecuteNonQueryAsync();
        }

        private static async Task SeedRolesAsync(string connectionString)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            // בדיקה האם טבלת התפקידים ריקה (כדי שהקוד לא יוסיף כפילויות בכל הרצה)
            string checkQuery = "SELECT COUNT(*) FROM Roles;";
            using var checkCommand = new MySqlCommand(checkQuery, connection);
            long count = Convert.ToInt64(await checkCommand.ExecuteScalarAsync());

            // אם הטבלה ריקה, נכניס את התפקידים המוגדרים במסמך הייזום
            if (count == 0)
            {
                string insertQuery = @"
                    INSERT INTO Roles (RoleName) VALUES 
                    ('Trainee'), 
                    ('Trainer'), 
                    ('SuperAdmin');";
                using var insertCommand = new MySqlCommand(insertQuery, connection);
                await insertCommand.ExecuteNonQueryAsync();
            }
        }
    }
}