namespace UserManagementAPI.Models;

/// <summary>
/// Data Transfer Object for creating or updating a user. 
/// Note: This was entirely CoPilot's idea
/// </summary>
public class UserDto
{
    public required string UserName { get; set; }
    public required int Age { get; set; }
    public required string Email { get; set; }
}