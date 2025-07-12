using BCrypt.Net;
using Microsoft.Data.Sqlite;

var connectionString = "Data Source=../JobTracker.API/job_tracker.db";
using var connection = new SqliteConnection(connectionString);
connection.Open();

// Check if user already exists
var checkCommand = connection.CreateCommand();
checkCommand.CommandText = "SELECT COUNT(*) FROM Users WHERE Email = @email";
checkCommand.Parameters.AddWithValue("@email", "test@example.com");
var count = (long)checkCommand.ExecuteScalar();

if (count > 0)
{
    Console.WriteLine("User already exists!");
    return;
}

// Hash the password
var password = "Test123!";
var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

// Create user
var userId = Guid.NewGuid();
var email = "test@example.com";
var fullName = "Test User";

var insertCommand = connection.CreateCommand();
insertCommand.CommandText = @"
    INSERT INTO Users (Id, Email, FullName, PasswordHash, CreatedAt, UpdatedAt, IsDeleted)
    VALUES (@id, @email, @fullName, @passwordHash, @createdAt, @updatedAt, @isDeleted)
";

insertCommand.Parameters.AddWithValue("@id", userId.ToString());
insertCommand.Parameters.AddWithValue("@email", email);
insertCommand.Parameters.AddWithValue("@fullName", fullName);
insertCommand.Parameters.AddWithValue("@passwordHash", hashedPassword);
insertCommand.Parameters.AddWithValue("@createdAt", DateTime.UtcNow.ToString("o"));
insertCommand.Parameters.AddWithValue("@updatedAt", DateTime.UtcNow.ToString("o"));
insertCommand.Parameters.AddWithValue("@isDeleted", 0);

try
{
    insertCommand.ExecuteNonQuery();
    Console.WriteLine($"User created successfully!");
    Console.WriteLine($"Email: {email}");
    Console.WriteLine($"Password: {password}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error creating user: {ex.Message}");
}