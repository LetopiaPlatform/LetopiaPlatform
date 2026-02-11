namespace LetopiaPlatform.API.DTOs.User;

public class UpdateUserProfileDto
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Bio { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public IFormFile? AvatarUrl { get; set; }
}
