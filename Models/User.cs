namespace UserManagementAPI.Models;

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