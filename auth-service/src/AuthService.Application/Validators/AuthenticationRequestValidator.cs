using AuthService.Application.DTOs.Request;
using AuthService.Domain.Interfaces;
using AuthService.Domain.ValueObjects;

namespace AuthService.Application.Validators;

public sealed class AuthenticationRequestValidator(IPasswordPolicy passwordPolicy)
{
    public IReadOnlyList<string> ValidateRegistration(RegisterAccountRequest request)
    {
        var errors = new List<string>();

        if (!EmailAddress.TryCreate(request.Email, out _))
        {
            errors.Add("Indica una dirección de correo válida.");
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName) || request.DisplayName.Trim().Length > 80)
        {
            errors.Add("El nombre debe tener entre 1 y 80 caracteres.");
        }

        errors.AddRange(passwordPolicy.GetViolations(request.Password));
        return errors;
    }

    public IReadOnlyList<string> ValidateSignIn(SignInRequest request)
    {
        var errors = new List<string>();

        if (!EmailAddress.TryCreate(request.Email, out _))
        {
            errors.Add("Indica una dirección de correo válida.");
        }

        if (string.IsNullOrEmpty(request.Password) || request.Password.Length > 128)
        {
            errors.Add("La contraseña es obligatoria y no puede superar 128 caracteres.");
        }

        return errors;
    }

    public IReadOnlyList<string> ValidateRefresh(RefreshSessionRequest request) =>
        ValidateRefreshToken(request.RefreshToken);

    public IReadOnlyList<string> ValidateRevoke(RevokeSessionRequest request) =>
        ValidateRefreshToken(request.RefreshToken);

    private static IReadOnlyList<string> ValidateRefreshToken(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken) || refreshToken.Length is < 32 or > 128)
        {
            return ["Indica un refresh token válido."];
        }

        return [];
    }
}