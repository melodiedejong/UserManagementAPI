// UserManagementAPI - Minimal ASP.NET Core API for managing users.
// Features: Authentication, input validation, error handling, and in-memory storage.

using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

/// Global exception handling middleware.
// Catches unhandled exceptions and returns a JSON error response.

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var errorResponse = System.Text.Json.JsonSerializer.Serialize(new { error = "Internal server error." });
        await context.Response.WriteAsync(errorResponse);
        // Optional: log the exception details
        Console.WriteLine($"Unhandled exception: {ex}");
    }
});

// Authentication middleware.
// Validates Bearer tokens in the Authorization header and returns 401 Unauthorized for invalid or missing tokens.

app.Use(async (context, next) =>
{
    // Example: Expect token in Authorization header as "Bearer <token>"
    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
    if (authHeader is null || !authHeader.StartsWith("Bearer "))
    {
        context.Response.StatusCode = 401;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"error\": \"Unauthorized: Missing or invalid token.\"}");
        return;
    }

    var token = authHeader.Substring("Bearer ".Length).Trim();

    // Simple token validation (replace with your real validation logic)
    var validTokens = new[] { "mysecrettoken123", "another-valid-token" };
    if (!validTokens.Contains(token))
    {
        context.Response.StatusCode = 401;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"error\": \"Unauthorized: Invalid token.\"}");
        return;
    }

    await next();
});

// Request/response logging middleware.
// Logs HTTP method, path, response status, and body for each request.

app.Use(async (context, next) =>
{
    // Log the incoming request
    Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");

    // Copy original response body stream
    var originalBodyStream = context.Response.Body;
    using var responseBody = new MemoryStream();
    context.Response.Body = responseBody;

    await next();

    // Read the response body
    context.Response.Body.Seek(0, SeekOrigin.Begin);
    var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
    context.Response.Body.Seek(0, SeekOrigin.Begin);

    // Log the response status code and body
    Console.WriteLine($"Response: {context.Response.StatusCode} {responseText}");

    // Copy the contents of the new memory stream (which contains the response) to the original stream
    await responseBody.CopyToAsync(originalBodyStream);
    context.Response.Body = originalBodyStream;
});

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

// Root endpoint
app.MapGet("/", () => Results.Ok("I am root"));

// GET /users - Retrieves all users.

app.MapGet("/users", () =>
{
    lock (usersLock)
    {
        return Results.Ok(usersById.Values.ToList());
    }
});

// GET /users/{id} - Retrieves a user by their unique ID.

app.MapGet("/users/{id:int}", (int id) =>
{
    lock (usersLock)
    {
        return usersById.TryGetValue(id, out var user)
            ? Results.Ok(user)
            : Results.NotFound();
    }
});

// GET /users/by-username/{username} - Retrieves a user by their username.

app.MapGet("/users/by-username/{username}", (string username) =>
{
    lock (usersLock)
    {
        return usersByUsername.TryGetValue(username, out var user)
            ? Results.Ok(user)
            : Results.NotFound();
    }
});

// POST /users - Creates a new user. Requires a unique username and email.

app.MapPost("/users", (UserDto userDto) =>
{
    if (userDto is null ||
        string.IsNullOrWhiteSpace(userDto.UserName) ||
        string.IsNullOrWhiteSpace(userDto.Email) ||
        !IsValidEmail(userDto.Email) ||
        userDto.Age < 0 ||
        userDto.UserName.Length > 256 ||
        userDto.Email.Length > 256)
    {
        return Results.BadRequest("Invalid user data. UserName and Email are required, must be valid, and max length is 256. Age must be non-negative.");
    }

    lock (usersLock)
    {
        if (usersByUsername.ContainsKey(userDto.UserName) ||
            usersByEmail.ContainsKey(userDto.Email))
        {
            return Results.Conflict("A user with the same username or email already exists.");
        }

        var user = new User
        {
            Id = nextId++,
            UserName = userDto.UserName,
            Age = userDto.Age,
            Email = userDto.Email
        };
        usersById[user.Id] = user;
        usersByUsername[user.UserName] = user;
        usersByEmail[user.Email] = user;
        return Results.Created($"/users/{user.Id}", user);
    }
});

// PUT /users/{id} - Updates an existing user by ID.

app.MapPut("/users/{id:int}", (int id, UserDto updatedUser) =>
{
    if (updatedUser is null ||
        string.IsNullOrWhiteSpace(updatedUser.UserName) ||
        string.IsNullOrWhiteSpace(updatedUser.Email) ||
        !IsValidEmail(updatedUser.Email) ||
        updatedUser.Age < 0 ||
        updatedUser.UserName.Length > 256 ||
        updatedUser.Email.Length > 256)
    {
        return Results.BadRequest("Invalid user data. UserName and Email are required, must be valid, and max length is 256. Age must be non-negative.");
    }

    lock (usersLock)
    {
        if (!usersById.TryGetValue(id, out var user))
            return Results.NotFound();

        // Check for duplicate username or email (case-insensitive), excluding the current user
        if ((usersByUsername.TryGetValue(updatedUser.UserName, out var userByName) && userByName.Id != id) ||
            (usersByEmail.TryGetValue(updatedUser.Email, out var userByEmail) && userByEmail.Id != id))
        {
            return Results.Conflict("A user with the same username or email already exists.");
        }

        // Remove old keys if username or email changed
        if (!user.UserName.Equals(updatedUser.UserName, StringComparison.OrdinalIgnoreCase))
            usersByUsername.Remove(user.UserName);
        if (!user.Email.Equals(updatedUser.Email, StringComparison.OrdinalIgnoreCase))
            usersByEmail.Remove(user.Email);

        user.UserName = updatedUser.UserName;
        user.Age = updatedUser.Age;
        user.Email = updatedUser.Email;

        usersByUsername[user.UserName] = user;
        usersByEmail[user.Email] = user;

        return Results.Ok(user);
    }
});

// DELETE /users/{id} - Deletes a user by ID.

app.MapDelete("/users/{id:int}", (int id) =>
{
    lock (usersLock)
    {
        if (!usersById.TryGetValue(id, out var user))
            return Results.NotFound();

        usersById.Remove(id);
        usersByUsername.Remove(user.UserName);
        usersByEmail.Remove(user.Email);

        return Results.NoContent();
    }
});

// GET /throw - Endpoint to test global error handling by throwing an exception.

app.MapGet("/throw", () => Results.Problem("Test exception"));

app.Run();

/// <summary>
/// Represents a user in the system.
/// </summary>
public class User
{
    public int Id { get; set; }
    public required string UserName { get; set; }
    public int Age { get; set; }
    public required string Email { get; set; }
}

/// <summary>
/// Data Transfer Object for creating or updating a user.
/// </summary>
public class UserDto
{
    public required string UserName { get; set; }
    public required int Age { get; set; }
    public required string Email { get; set; }
}