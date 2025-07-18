// src/Application/DTOs/AuthDTOs.cs
using System.ComponentModel.DataAnnotations;

namespace CleanEfApi.Application.DTOs;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string? Password { get; set; }
}

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    public string? Password { get; set; }

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string? ConfirmPassword { get; set; }
}

public class AuthResponse
{
    public string? Token { get; set; }
    public string? Message { get; set; }
    public bool Success { get; set; }
    public List<string>? Errors { get; set; } // For error messages
}