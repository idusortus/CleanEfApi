namespace CleanEfApi.Application.DTOs;

public class LoginRequest
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}

public class RegisterRequest
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
}

public class AuthResponse
{
    public string? Token { get; set; }
    public string? Message { get; set; }
    public bool Success { get; set; }
    public List<string>? Errors { get; set; } // For error messages
}