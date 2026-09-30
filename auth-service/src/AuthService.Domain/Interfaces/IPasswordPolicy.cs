namespace AuthService.Domain.Interfaces;

public interface IPasswordPolicy
{
    IReadOnlyList<string> GetViolations(string password);
}