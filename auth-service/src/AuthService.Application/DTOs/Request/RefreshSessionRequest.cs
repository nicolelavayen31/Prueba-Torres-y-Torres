using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.DTOs.Request;

public sealed class RefreshSessionRequest
{
    [Required, StringLength(128, MinimumLength = 32)]
    public string RefreshToken { get; init; } = string.Empty;
}