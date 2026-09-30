using AuthService.Shared.Enums;

namespace AuthService.Shared.Wrappers;

public sealed class OperationResult<T>
{
    private OperationResult(bool succeeded, T? value, AuthErrorCode? errorCode, string? errorMessage)
    {
        Succeeded = succeeded;
        Value = value;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public bool Succeeded { get; }
    public T? Value { get; }
    public AuthErrorCode? ErrorCode { get; }
    public string? ErrorMessage { get; }

    public static OperationResult<T> Success(T value) => new(true, value, null, null);

    public static OperationResult<T> Failure(AuthErrorCode code, string message) =>
        new(false, default, code, message);
}