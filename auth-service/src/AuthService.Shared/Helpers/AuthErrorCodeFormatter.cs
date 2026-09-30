using AuthService.Shared.Enums;

namespace AuthService.Shared.Helpers;

public static class AuthErrorCodeFormatter
{
    public static string ToWireCode(this AuthErrorCode code) => code switch
    {
        AuthErrorCode.InvalidInput => "invalid_input",
        AuthErrorCode.EmailAlreadyInUse => "email_already_in_use",
        AuthErrorCode.InvalidCredentials => "invalid_credentials",
        AuthErrorCode.RefreshTokenRejected => "refresh_token_rejected",
        _ => "authentication_error"
    };
}