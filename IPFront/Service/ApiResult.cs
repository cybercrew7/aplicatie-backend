using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.AspNetCore.Components.Forms;
using Shared.DTO;

namespace IPFront.Service
{
    /// <summary>
    /// Outcome of a backend API call that returns no body (e.g. 204 No Content).
    /// On failure, <see cref="Error"/> holds the problem details sent by the server
    /// (title + field-keyed messages), or a client-side fallback when the body could
    /// not be read or the server could not be reached.
    /// </summary>
    public class ApiResult
    {
        /// <summary>Creates a result; use <see cref="Success"/> or <see cref="Failure"/>.</summary>
        protected ApiResult(HttpStatusCode? statusCode, ApiErrorDTO? error)
        {
            StatusCode = statusCode;
            Error = error;
        }

        /// <summary>HTTP status of the response, or null when no response was received (network failure).</summary>
        public HttpStatusCode? StatusCode { get; }

        /// <summary>Error details when the call failed; null on success.</summary>
        public ApiErrorDTO? Error { get; }

        /// <summary>True when the API answered with a 2xx status.</summary>
        [MemberNotNullWhen(false, nameof(Error))]
        public bool IsSuccess => Error is null;

        /// <summary>True when the API answered 409 Conflict (e.g. username or email already taken).</summary>
        public bool IsConflict => StatusCode == HttpStatusCode.Conflict;

        /// <summary>Human-readable summary of the failure (the problem details <c>title</c>); null on success.</summary>
        public string? ErrorMessage => Error?.title;

        /// <summary>
        /// Messages for one field, keyed by DTO property name (e.g. <c>"user_name"</c>, <c>"email"</c>).
        /// Lookup is case-insensitive. Returns an empty list when the field has no errors.
        /// </summary>
        public IReadOnlyList<string> GetFieldErrors(string field)
            => Error is not null && Error.errors.TryGetValue(field, out var messages) ? messages : [];

        /// <summary>
        /// Copies the server's field errors into an <see cref="EditForm"/> validation store so each message
        /// shows next to its input via <c>&lt;ValidationMessage&gt;</c>. Errors that do not belong to a field
        /// are attached to the model and appear in <c>&lt;ValidationSummary&gt;</c>.
        /// The caller should clear <paramref name="store"/> before each submit.
        /// </summary>
        public void ApplyTo(EditContext editContext, ValidationMessageStore store)
        {
            if (Error is null)
            {
                return;
            }

            foreach (var (field, messages) in Error.errors)
            {
                store.Add(new FieldIdentifier(editContext.Model, field), messages);
            }
            editContext.NotifyValidationStateChanged();
        }

        /// <summary>Successful result.</summary>
        public static ApiResult Success(HttpStatusCode statusCode) => new(statusCode, null);

        /// <summary>Failed result.</summary>
        public static ApiResult Failure(HttpStatusCode? statusCode, ApiErrorDTO error) => new(statusCode, error);
    }

    /// <summary>Outcome of a backend API call that returns a <typeparamref name="T"/> body on success.</summary>
    public sealed class ApiResult<T> : ApiResult
    {
        private ApiResult(HttpStatusCode? statusCode, T? value, ApiErrorDTO? error)
            : base(statusCode, error)
        {
            Value = value;
        }

        /// <summary>The response body on success; default otherwise.</summary>
        public T? Value { get; }

        /// <summary>Error details when the call failed; null on success.</summary>
        /// <remarks>Re-declared so <see cref="IsSuccess"/> can annotate it for null-state analysis.</remarks>
        public new ApiErrorDTO? Error => base.Error;

        /// <summary>True when the API answered with a 2xx status and a body.</summary>
        [MemberNotNullWhen(true, nameof(Value))]
        [MemberNotNullWhen(false, nameof(Error))]
        public new bool IsSuccess => base.IsSuccess;

        /// <summary>Successful result carrying <paramref name="value"/>.</summary>
        public static ApiResult<T> Success(HttpStatusCode statusCode, T value) => new(statusCode, value, null);

        /// <summary>Failed result.</summary>
        public static new ApiResult<T> Failure(HttpStatusCode? statusCode, ApiErrorDTO error) => new(statusCode, default, error);
    }
}
