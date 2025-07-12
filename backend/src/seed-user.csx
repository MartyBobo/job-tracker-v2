#r "nuget: BCrypt.Net-Next, 4.0.3"
#r "nuget: Microsoft.Data.Sqlite, 8.0.0"

using BCrypt.Net;
using Microsoft.Data.Sqlite;
using System;

var connectionString = "Data Source=jobtracker.db";
using var connection = new SqliteConnection(connectionString);
connection.Open();

// Hash the password
var password = "Test123!";
var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

// Create user
var userId = Guid.NewGuid();
var email = "test@example.com";
var firstName = "Test";
var lastName = "User";

var insertCommand = connection.CreateCommand();
insertCommand.CommandText = @"
    INSERT INTO Users (Id, Email, FirstName, LastName, PasswordHash, CreatedAt, UpdatedAt, IsActive)
    VALUES (@id, @email, @firstName, @lastName, @passwordHash, @createdAt, @updatedAt, @isActive)
";

insertCommand.Parameters.AddWithValue("@id", userId.ToString());
insertCommand.Parameters.AddWithValue("@email", email);
insertCommand.Parameters.AddWithValue("@firstName", firstName);
insertCommand.Parameters.AddWithValue("@lastName", lastName);
insertCommand.Parameters.AddWithValue("@passwordHash", hashedPassword);
insertCommand.Parameters.AddWithValue("@createdAt", DateTime.UtcNow.ToString("o"));
insertCommand.Parameters.AddWithValue("@updatedAt", DateTime.UtcNow.ToString("o"));
insertCommand.Parameters.AddWithValue("@isActive", 1);

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