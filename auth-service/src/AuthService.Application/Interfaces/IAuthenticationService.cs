using AuthService.Application.DTOs.Request;
using AuthService.Application.DTOs.Response;
using AuthService.Shared.Wrappers;

namespace AuthService.Application.Interfaces;

public interface IAuthenticationService
{
    Task<OperationResult<AuthSessionDto>> RegisterAsync(
        RegisterAccountRequest request,
        CancellationToken cancellationToken);

    Task<OperationResult<AuthSessionDto>> SignInAsync(
        SignInRequest request,
        CancellationToken cancellationToken);

    Task<OperationResult<AuthSessionDto>> RefreshAsync(
        RefreshSessionRequest request,
        CancellationToken cancellationToken);

    Task<OperationResult<bool>> RevokeAsync(
        RevokeSessionRequest request,
        int accountId,
        CancellationToken cancellationToken);
}