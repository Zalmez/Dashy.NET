namespace dashy3.ApiService.Dtos;

public record UserDto(
    string Id,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    string[] Roles,
    DateTime CreatedAt,
    DateTime? LastLoginAt);

public record LoginRequest(string Email, string Password);

public record RegisterRequest(
    string Email,
    string Password,
    string DisplayName,
    string Role = "User");

public record UpdateProfileRequest(string DisplayName, string? AvatarUrl);

public record ChangePasswordRequest(string UserId, string CurrentPassword, string NewPassword);

public record OidcLoginRequest(string Sub, string? Email, string? Name, string? AvatarUrl);

public record UpdateUserRoleRequest(string Role, string Action); // Action: "add" or "remove"
