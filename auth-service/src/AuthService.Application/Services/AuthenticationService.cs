using AuthService.Application.DTOs.Request;
using AuthService.Application.DTOs.Response;
using AuthService.Application.Interfaces;
using AuthService.Application.Mappings;
using AuthService.Application.Validators;
using AuthService.Domain.Entities;
using AuthService.Domain.ValueObjects;
using AuthService.Shared.Enums;
using AuthService.Shared.Wrappers;

namespace AuthService.Application.Services;

public sealed class AuthenticationService(
    IAccountDirectory accountDirectory,
    IAccountRepository accountRepository,
    IPasswordHasher passwordHasher,
    IAccessTokenIssuer accessTokenIssuer,
    IRefreshTokenIssuer refreshTokenIssuer,
    IRefreshSessionStore refreshSessionStore,
    IAccountNotifier accountNotifier,
    AuthenticationRequestValidator validator,
    TimeProvider timeProvider) : IAuthenticationService
{
    public async Task<OperationResult<AuthSessionDto>> RegisterAsync(
        RegisterAccountRequest request,
        CancellationToken cancellationToken)
    {
        var errors = validator.ValidateRegistration(request);
        if (errors.Count > 0)
        {
            return OperationResult<AuthSessionDto>.Failure(AuthErrorCode.InvalidInput, string.Join(" ", errors));
        }

        var email = EmailAddress.Create(request.Email);
        if (await accountDirectory.FindByEmailAsync(email.Value, cancellationToken) is not null)
        {
            return OperationResult<AuthSessionDto>.Failure(
                AuthErrorCode.EmailAlreadyInUse,
                "Ya existe una cuenta con ese correo.");
        }

        var account = Account.Register(
            email,
            request.DisplayName,
            passwordHasher.Hash(request.Password),
            timeProvider.GetUtcNow());
        var persistedAccount = await accountRepository.TryAddAsync(account, cancellationToken);
        if (persistedAccount is null)
        {
            return OperationResult<AuthSessionDto>.Failure(
                AuthErrorCode.EmailAlreadyInUse,
                "Ya existe una cuenta con ese correo.");
        }

        await accountNotifier.SendWelcomeAsync(
            persistedAccount.Email.Value,
            persistedAccount.DisplayName,
            cancellationToken);
        var session = await CreateSessionAsync(persistedAccount, cancellationToken);
    return OperationResult<AuthSessionDto>.Success(session);
    }

    public async Task<OperationResult<AuthSessionDto>> SignInAsync(
        SignInRequest request,
        CancellationToken cancellationToken)
    {
        var errors = validator.ValidateSignIn(request);
        if (errors.Count > 0)
        {
            return OperationResult<AuthSessionDto>.Failure(AuthErrorCode.InvalidInput, string.Join(" ", errors));
        }

        var email = EmailAddress.Create(request.Email);
        var account = await accountDirectory.FindByEmailAsync(email.Value, cancellationToken);
        if (account is null || !passwordHasher.Verify(request.Password, account.PasswordHash))
        {
            return OperationResult<AuthSessionDto>.Failure(
                AuthErrorCode.InvalidCredentials,
                "El correo o la contraseña no son correctos.");
        }

        var session = await CreateSessionAsync(account, cancellationToken);
        return OperationResult<AuthSessionDto>.Success(session);
    }

    public async Task<OperationResult<AuthSessionDto>> RefreshAsync(
        RefreshSessionRequest request,
        CancellationToken cancellationToken)
    {
        var errors = validator.ValidateRefresh(request);
        if (errors.Count > 0)
        {
            return OperationResult<AuthSessionDto>.Failure(AuthErrorCode.InvalidInput, string.Join(" ", errors));
        }

        var now = timeProvider.GetUtcNow();
        var digest = refreshTokenIssuer.ComputeDigest(request.RefreshToken);
        var consumedSession = await refreshSessionStore.TryConsumeAsync(digest, null, now, cancellationToken);
        if (consumedSession is null)
        {
            return OperationResult<AuthSessionDto>.Failure(
                AuthErrorCode.RefreshTokenRejected,
                "El refresh token no es válido, expiró o ya fue utilizado.");
        }

        var account = await accountDirectory.FindByIdAsync(consumedSession.AccountId, cancellationToken);
        if (account is null)
        {
            return OperationResult<AuthSessionDto>.Failure(
                AuthErrorCode.RefreshTokenRejected,
                "El refresh token no es válido, expiró o ya fue utilizado.");
        }

        var session = await CreateSessionAsync(account, cancellationToken);
        return OperationResult<AuthSessionDto>.Success(session);
    }

    public async Task<OperationResult<bool>> RevokeAsync(
        RevokeSessionRequest request,
        int accountId,
        CancellationToken cancellationToken)
    {
        var errors = validator.ValidateRevoke(request);
        if (errors.Count > 0)
        {
            return OperationResult<bool>.Failure(AuthErrorCode.InvalidInput, string.Join(" ", errors));
        }

        var digest = refreshTokenIssuer.ComputeDigest(request.RefreshToken);
        var consumedSession = await refreshSessionStore.TryConsumeAsync(
            digest,
            accountId,
            timeProvider.GetUtcNow(),
            cancellationToken);
        if (consumedSession is null)
        {
            return OperationResult<bool>.Failure(
                AuthErrorCode.RefreshTokenRejected,
                "El refresh token no está activo para esta cuenta.");
        }

        return OperationResult<bool>.Success(true);
    }

    private async Task<AuthSessionDto> CreateSessionAsync(Account account, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var accessToken = accessTokenIssuer.IssueFor(account);
        var refreshToken = refreshTokenIssuer.Create(now);
        var refreshSession = RefreshSession.Start(
            account.Id,
            refreshToken.Digest,
            now,
            refreshToken.ExpiresAtUtc);

        if (!await refreshSessionStore.TryAddAsync(refreshSession, cancellationToken))
        {
            throw new InvalidOperationException("No fue posible guardar la sesión de autenticación.");
        }

        return AuthSessionMapper.ToDto(account, accessToken, refreshToken.Value, refreshToken.ExpiresAtUtc);
    }
}