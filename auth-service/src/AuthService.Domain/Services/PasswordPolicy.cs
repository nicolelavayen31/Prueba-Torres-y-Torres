using AuthService.Domain.Interfaces;

namespace AuthService.Domain.Services;

public sealed class PasswordPolicy : IPasswordPolicy
{
    public IReadOnlyList<string> GetViolations(string password)
    {
        var violations = new List<string>();

        if (password.Length < 12)
        {
            violations.Add("La contraseña debe tener al menos 12 caracteres.");
        }

        if (password.Length > 128)
        {
            violations.Add("La contraseña no puede superar 128 caracteres.");
        }

        if (!password.Any(char.IsUpper) || !password.Any(char.IsLower) || !password.Any(char.IsDigit))
        {
            violations.Add("La contraseña debe incluir mayúsculas, minúsculas y números.");
        }

        return violations;
    }
}