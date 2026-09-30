namespace AuthService.Shared.Wrappers;

public sealed record ApiResponse<T>(bool Succeeded, T? Data, ApiError? Error)
{
    public static ApiResponse<T> Success(T data) => new(true, data, null);

    public static ApiResponse<T> Failure(string code, string message) =>
        new(false, default, new ApiError(code, message));
}

public sealed record ApiError(string Code, string Message);