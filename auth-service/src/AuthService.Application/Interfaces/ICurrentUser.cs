namespace AuthService.Application.Interfaces;

public interface ICurrentUser
{
    int? AccountId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}