namespace saxafon.Services
{
    /// <summary>Outcome category of a user-account operation; the controller maps it to an HTTP status.</summary>
    public enum UserOperationStatus
    {
        /// <summary>Operation succeeded (200 / 204).</summary>
        Success,
        /// <summary>The user does not exist or is soft-deleted (404).</summary>
        NotFound,
        /// <summary>A uniqueness rule was violated, e.g. username or email already taken (409).</summary>
        Conflict,
        /// <summary>The request was well-formed but failed a business rule, e.g. wrong current password (400).</summary>
        ValidationFailed
    }

    /// <summary>
    /// Structured result for user-account operations. Failures carry field-keyed error
    /// messages (keys are DTO property names such as "user_name" or "email") so the client
    /// can show each message next to the offending input.
    /// </summary>
    public sealed class UserOperationResult<T>
    {
        private static readonly IReadOnlyDictionary<string, string[]> NoErrors = new Dictionary<string, string[]>();

        private UserOperationResult(UserOperationStatus status, T? value, string? message, IReadOnlyDictionary<string, string[]>? errors)
        {
            Status = status;
            Value = value;
            Message = message;
            Errors = errors ?? NoErrors;
        }

        /// <summary>Outcome category.</summary>
        public UserOperationStatus Status { get; }
        /// <summary>Payload on success; default otherwise.</summary>
        public T? Value { get; }
        /// <summary>Human-readable summary of the failure.</summary>
        public string? Message { get; }
        /// <summary>Field-level error messages keyed by DTO property name.</summary>
        public IReadOnlyDictionary<string, string[]> Errors { get; }
        /// <summary>True when <see cref="Status"/> is <see cref="UserOperationStatus.Success"/>.</summary>
        public bool IsSuccess => Status == UserOperationStatus.Success;

        /// <summary>Successful result carrying <paramref name="value"/>.</summary>
        public static UserOperationResult<T> Success(T value) => new(UserOperationStatus.Success, value, null, null);

        /// <summary>The user was not found.</summary>
        public static UserOperationResult<T> NotFound(string message) => new(UserOperationStatus.NotFound, default, message, null);

        /// <summary>One or more uniqueness conflicts, keyed by field.</summary>
        public static UserOperationResult<T> Conflict(string message, IReadOnlyDictionary<string, string[]> errors) =>
            new(UserOperationStatus.Conflict, default, message, errors);

        /// <summary>A business-rule validation failure on a single field.</summary>
        public static UserOperationResult<T> ValidationFailed(string field, string message) =>
            new(UserOperationStatus.ValidationFailed, default, message, new Dictionary<string, string[]> { [field] = new[] { message } });
    }
}
