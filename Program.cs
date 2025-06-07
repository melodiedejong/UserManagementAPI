// UserManagementAPI - Minimal ASP.NET Core API for managing users.
// Features: Authentication, input validation, error handling, and in-memory storage.

using System.Text.RegularExpressions;
using UserManagementAPI.Models;
using UserManagementAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Configure middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<AuthenticationMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// In-memory user dictionaries for fast lookups
var usersById = new Dictionary<int, User>();
var usersByUsername = new Dictionary<string, User>(StringComparer.OrdinalIgnoreCase);
var usersByEmail = new Dictionary<string, User>(StringComparer.OrdinalIgnoreCase);
var nextId = 1;
var usersLock = new object();

/// <summary>
/// Validates the format of an email address.
/// </summary>
/// <param name="email">The email address to validate.</param>
/// <returns>True if the email is valid; otherwise, false.</returns>

bool IsValidEmail(string email)
{
    if (string.IsNullOrWhiteSpace(email))
        return false;
    var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    return emailRegex.IsMatch(email);
}

// Register endpoints from Endpoints.cs
Endpoints.MapUserEndpoints(
    app,
    usersById,
    usersByUsername,
    usersByEmail,
    usersLock,
    IsValidEmail,
    () => nextId++
);

app.Run();

