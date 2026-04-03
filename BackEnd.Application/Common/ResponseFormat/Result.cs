namespace BackEnd.Application.Common.ResponseFormat
{
    public interface IResult
    {
        bool IsSuccess { get; }
        string Message { get; }
        ErrorType ErrorType { get; }
        object Value { get; }
    }

    public class Result<T> : IResult
    {
        public bool IsSuccess { get; }
        public T Value { get; }
        public string Message { get; }
        public ErrorType ErrorType { get; }
        public Dictionary<string, string[]>? Errors { get; }  // ✅ جديد

        object IResult.Value => Value!;

        // ── Success constructor ───────────────────────────────
        private Result(T value, string? message = null)
        {
            IsSuccess = true;
            Value = value;
            Message = message ?? "Operation completed successfully.";
            ErrorType = ErrorType.None;
        }

        // ── Failure constructor بدون errors ──────────────────
        private Result(string message, ErrorType errorType)
        {
            IsSuccess = false;
            Value = default!;
            Message = message;
            ErrorType = errorType;
        }

        // ── Failure constructor مع errors ────────────────────
        private Result(string message, ErrorType errorType,
            Dictionary<string, string[]>? errors)
        {
            IsSuccess = false;
            Value = default!;
            Message = message;
            ErrorType = errorType;
            Errors = errors;
        }

        // ── Static Factory Methods ────────────────────────────
        public static Result<T> Success(T value, string? message = null)
            => new(value, message);

        public static Result<T> Failure(string message, ErrorType errorType)
            => new(message, errorType);

        // ✅ الـ overload الجديد
        public static Result<T> Failure(
            string message,
            ErrorType errorType,
            Dictionary<string, string[]>? errors)
            => new(message, errorType, errors);
    }

    public enum ErrorType
    {
        None,
        NotFound,
        BadRequest,
        Conflict,
        UnprocessableEntity,
        InternalServerError,
        Unauthorized,
        Forbidden,
        ServerError

    }
}