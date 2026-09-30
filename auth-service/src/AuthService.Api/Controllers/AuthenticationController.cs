using AuthService.Application.DTOs.Request;
using AuthService.Application.DTOs.Response;
using AuthService.Application.Interfaces;
using AuthService.Shared.Enums;
using AuthService.Shared.Helpers;
using AuthService.Shared.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthenticationController(
    IAuthenticationService authenticationService,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthSessionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        RegisterAccountRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authenticationService.RegisterAsync(request, cancellationToken);
        if (!result.Succeeded)
        {
            return ErrorResponse(result.ErrorCode, result.ErrorMessage);
        }

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<AuthSessionDto>.Success(result.Value!));
    }

    [HttpPost("sign-in")]
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthSessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SignIn(SignInRequest request, CancellationToken cancellationToken)
    {
        var result = await authenticationService.SignInAsync(request, cancellationToken);
        if (!result.Succeeded)
        {
            return ErrorResponse(result.ErrorCode, result.ErrorMessage);
        }

        return Ok(ApiResponse<AuthSessionDto>.Success(result.Value!));
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<AuthSessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken(
        RefreshSessionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authenticationService.RefreshAsync(request, cancellationToken);
        if (!result.Succeeded)
        {
            return ErrorResponse(result.ErrorCode, result.ErrorMessage);
        }

        return Ok(ApiResponse<AuthSessionDto>.Success(result.Value!));
    }

    [Authorize]
    [HttpPost("revoke-token")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RevokeToken(
        RevokeSessionRequest request,
        CancellationToken cancellationToken)
    {
        if (currentUser.AccountId is not int accountId)
        {
            return Unauthorized();
        }

        var result = await authenticationService.RevokeAsync(request, accountId, cancellationToken);
        if (!result.Succeeded)
        {
            return ErrorResponse(result.ErrorCode, result.ErrorMessage);
        }

        return Ok(ApiResponse<bool>.Success(result.Value));
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<CurrentAccountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Me()
    {
        if (currentUser.AccountId is not int accountId)
        {
            return Unauthorized();
        }

        return Ok(ApiResponse<CurrentAccountDto>.Success(new CurrentAccountDto(accountId, currentUser.Email)));
    }

    private ObjectResult ErrorResponse(AuthErrorCode? errorCode, string? message)
    {
        var code = errorCode ?? AuthErrorCode.InvalidInput;
        var statusCode = code switch
        {
            AuthErrorCode.InvalidInput => StatusCodes.Status400BadRequest,
            AuthErrorCode.EmailAlreadyInUse => StatusCodes.Status409Conflict,
            AuthErrorCode.InvalidCredentials => StatusCodes.Status401Unauthorized,
            AuthErrorCode.RefreshTokenRejected => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status400BadRequest
        };
        var response = ApiResponse<object>.Failure(code.ToWireCode(), message ?? "La solicitud no pudo procesarse.");
        return StatusCode(statusCode, response);
    }
}