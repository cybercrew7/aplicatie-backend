using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Shared.DTO;

namespace IPFront.Service
{
    /// <summary>Typed HTTP client for the <c>api/User</c> endpoints.</summary>
    public class UserApiClient
    {
        private const string UnreachableMessage = "Could not reach the server. Check your connection and try again.";
        private const int MaxPlainTextErrorLength = 500;

        private readonly HttpClient httpClient;

        /// <summary>Creates the client; the base address is configured in <c>Program.cs</c>.</summary>
        public UserApiClient(HttpClient _httpClient)
        {
            httpClient = _httpClient;
        }

        /// <summary>Returns all users.</summary>
        public async Task<IEnumerable<UserMinDTO>> getUsersAsync()
        {
            return await httpClient.GetFromJsonAsync<IEnumerable<UserMinDTO>>("api/User/getUser") ?? [];
        }

        /// <summary>Returns a user by id. Throws <see cref="HttpRequestException"/> on a non-success status.</summary>
        public async Task<UserMinDTO?> getUserByIdAsync(int id)
        {
            return await httpClient.GetFromJsonAsync<UserMinDTO>($"api/User/getUserId/{id}");
        }

        /// <summary>Registers a new user.</summary>
        public async Task<UserMinDTO> createUserAsync(UserRegisterDTO userRegisterDTO)
        {
            var response = await httpClient.PostAsJsonAsync("api/User/register", userRegisterDTO);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserMinDTO>()
                ?? throw new InvalidOperationException("The server returned an empty registration response.");
        }

        /// <summary>Authenticates a user.</summary>
        public async Task<UserResponseAuthDTO> loginUserAsync(UserLoginDTO userLogin)
        {
            var response = await httpClient.PostAsJsonAsync("api/User/login", userLogin);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Login failed");
            }
            return await response.Content.ReadFromJsonAsync<UserResponseAuthDTO>()
                ?? throw new InvalidOperationException("The server returned an empty login response.");
        }


        // ------------------------------------------------------------------
        // Profile & account settings
        //
        // These methods never throw for HTTP errors: they return an ApiResult whose
        // Error carries the server's problem details (title + field-keyed messages),
        // so the UI can show e.g. "Username is already taken." next to the input.
        // ------------------------------------------------------------------

        /// <summary>GET <c>api/User/{id}/profile</c> — full profile and settings.</summary>
        public Task<ApiResult<UserProfileDTO>> GetUserProfileAsync(int userId, CancellationToken cancellationToken = default)
            => SendAsync<UserProfileDTO>(() => httpClient.GetAsync($"api/User/{userId}/profile", cancellationToken), cancellationToken);

        /// <summary>
        /// PUT <c>api/User/{id}/profile</c> — names, username, email, phone.
        /// Fails with 409 when the username and/or email belong to another account.
        /// </summary>
        public Task<ApiResult<UserProfileDTO>> UpdateUserProfileAsync(int userId, UpdateUserProfileDTO profile, CancellationToken cancellationToken = default)
            => SendAsync<UserProfileDTO>(() => httpClient.PutAsJsonAsync($"api/User/{userId}/profile", profile, cancellationToken), cancellationToken);

        /// <summary>PUT <c>api/User/{id}/settings</c> — notification and appearance preferences.</summary>
        public Task<ApiResult<UserProfileDTO>> UpdateUserSettingsAsync(int userId, UpdateUserSettingsDTO settings, CancellationToken cancellationToken = default)
            => SendAsync<UserProfileDTO>(() => httpClient.PutAsJsonAsync($"api/User/{userId}/settings", settings, cancellationToken), cancellationToken);

        /// <summary>
        /// PUT <c>api/User/{id}/password</c>. Fails with 400 when the current password is wrong
        /// (see <c>errors.current_password</c>).
        /// </summary>
        public async Task<ApiResult> ChangePasswordAsync(int userId, ChangePasswordDTO password, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response;
            try
            {
                response = await httpClient.PutAsJsonAsync($"api/User/{userId}/password", password, cancellationToken);
            }
            catch (HttpRequestException)
            {
                return ApiResult.Failure(null, UnreachableError());
            }

            using (response)
            {
                return response.IsSuccessStatusCode
                    ? ApiResult.Success(response.StatusCode)
                    : ApiResult.Failure(response.StatusCode, await ReadErrorAsync(response, cancellationToken));
            }
        }


        private static async Task<ApiResult<T>> SendAsync<T>(Func<Task<HttpResponseMessage>> send, CancellationToken cancellationToken)
        {
            HttpResponseMessage response;
            try
            {
                response = await send();
            }
            catch (HttpRequestException)
            {
                return ApiResult<T>.Failure(null, UnreachableError());
            }

            using (response)
            {
                if (!response.IsSuccessStatusCode)
                {
                    return ApiResult<T>.Failure(response.StatusCode, await ReadErrorAsync(response, cancellationToken));
                }

                T? value;
                try
                {
                    value = await response.Content.ReadFromJsonAsync<T>(cancellationToken);
                }
                catch (JsonException)
                {
                    value = default;
                }

                return value is null
                    ? ApiResult<T>.Failure(response.StatusCode, new ApiErrorDTO
                    {
                        title = "The server returned an unexpected response.",
                        status = (int)response.StatusCode
                    })
                    : ApiResult<T>.Success(response.StatusCode, value);
            }
        }

        /// <summary>
        /// Reads the error body. The API sends ValidationProblemDetails (application/problem+json)
        /// for 400/404/409; anything else (plain text, HTML error page, empty body) falls back to a
        /// generic message for the status code.
        /// </summary>
        private static async Task<ApiErrorDTO> ReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            ApiErrorDTO? error = null;
            var mediaType = response.Content.Headers.ContentType?.MediaType;

            if (mediaType is not null && mediaType.Contains("json", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    error = await response.Content.ReadFromJsonAsync<ApiErrorDTO>(cancellationToken);
                }
                catch (JsonException)
                {
                    error = null;
                }
            }
            else if (string.Equals(mediaType, "text/plain", StringComparison.OrdinalIgnoreCase))
            {
                var text = (await response.Content.ReadAsStringAsync(cancellationToken)).Trim();
                if (text.Length is > 0 and <= MaxPlainTextErrorLength)
                {
                    error = new ApiErrorDTO { title = text };
                }
            }

            error ??= new ApiErrorDTO();
            error.status ??= (int)response.StatusCode;
            error.errors = NormalizeFieldErrors(error.errors);
            if (string.IsNullOrWhiteSpace(error.title))
            {
                error.title = DefaultMessage(response.StatusCode);
            }
            return error;
        }

        /// <summary>
        /// Makes field lookups case-insensitive and strips the "$." prefix that ASP.NET Core
        /// uses for JSON-binding errors, so keys match DTO property names.
        /// </summary>
        private static Dictionary<string, string[]> NormalizeFieldErrors(Dictionary<string, string[]>? errors)
        {
            var normalized = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            if (errors is null)
            {
                return normalized;
            }

            foreach (var (key, messages) in errors)
            {
                var field = key.StartsWith("$.", StringComparison.Ordinal) ? key[2..] : key;
                normalized[field] = normalized.TryGetValue(field, out var existing)
                    ? [.. existing, .. messages ?? []]
                    : messages ?? [];
            }
            return normalized;
        }

        private static ApiErrorDTO UnreachableError() => new() { title = UnreachableMessage };

        private static string DefaultMessage(HttpStatusCode statusCode) => statusCode switch
        {
            HttpStatusCode.BadRequest => "The request was not valid.",
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => "You are not allowed to perform this action.",
            HttpStatusCode.NotFound => "The requested user was not found.",
            HttpStatusCode.Conflict => "The data conflicts with an existing account.",
            >= HttpStatusCode.InternalServerError => "The server encountered an error. Please try again later.",
            _ => $"Request failed ({(int)statusCode})."
        };
    }
}
