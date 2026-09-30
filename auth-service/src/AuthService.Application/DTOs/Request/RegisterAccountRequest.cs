using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.DTOs.Request;

public sealed class RegisterAccountRequest
{
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; init; } = string.Empty;

    [Required, StringLength(80, MinimumLength = 1)]
    public string DisplayName { get; init; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 12)]
    public string Password { get; init; } = string.Empty;
}