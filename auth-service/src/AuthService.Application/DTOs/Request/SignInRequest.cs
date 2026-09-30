using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.DTOs.Request;

public sealed class SignInRequest
{
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; init; } = string.Empty;

    [Required, StringLength(128)]
    public string Password { get; init; } = string.Empty;
}