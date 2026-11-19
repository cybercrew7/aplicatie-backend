using Shared.DTO;

namespace IPFront.Service
{
    /// <summary>
    /// Holds the identity of the user currently using the app and keeps it in sync
    /// across components. Components that display user data subscribe to
    /// <see cref="OnChange"/> to re-render when the profile is updated.
    /// There is no login yet, so a development user id is used.
    /// TODO: set UserId from the login response once authentication is implemented.
    /// </summary>
    public class CurrentUserService
    {
        private readonly UserApiClient userApi;
        private UserMinDTO? currentUser;
        private Task<UserMinDTO?>? loadTask;

        /// <summary>Creates the service.</summary>
        public CurrentUserService(UserApiClient _userApi)
        {
            userApi = _userApi;
        }

        /// <summary>
        /// Raised after the cached user changes (e.g. after a successful profile update).
        /// Handlers may run outside the renderer's sync context, so components should
        /// call <c>InvokeAsync(StateHasChanged)</c>.
        /// </summary>
        public event Action? OnChange;

        /// <summary>Id of the current user.</summary>
        public int UserId { get; private set; } = 1;

        /// <summary>The cached user, or null if not loaded yet or the load failed.</summary>
        public UserMinDTO? CurrentUser => currentUser;

        /// <summary>
        /// Returns the cached user, loading it on first use. Concurrent callers share
        /// one request; a failed load is retried on the next call.
        /// </summary>
        public Task<UserMinDTO?> GetCurrentUserAsync()
        {
            if (currentUser is not null)
            {
                return Task.FromResult<UserMinDTO?>(currentUser);
            }

            if (loadTask is null || loadTask.IsCompleted)
            {
                loadTask = LoadCurrentUserAsync();
            }
            return loadTask;
        }

        /// <summary>Fetches the current user's full profile; on success also refreshes the cached user.</summary>
        public async Task<ApiResult<UserProfileDTO>> GetProfileAsync()
        {
            var result = await userApi.GetUserProfileAsync(UserId);
            if (result.IsSuccess)
            {
                ApplyProfile(result.Value);
            }
            return result;
        }

        /// <summary>
        /// Updates the current user's profile. On success the cached user is replaced and
        /// <see cref="OnChange"/> fires, so the layout shows the new name immediately.
        /// On 409 the result carries the conflicting fields (<c>user_name</c> / <c>email</c>).
        /// </summary>
        public async Task<ApiResult<UserProfileDTO>> UpdateProfileAsync(UpdateUserProfileDTO profile)
        {
            var result = await userApi.UpdateUserProfileAsync(UserId, profile);
            if (result.IsSuccess)
            {
                ApplyProfile(result.Value);
            }
            return result;
        }

        /// <summary>Updates the current user's notification and appearance settings.</summary>
        public async Task<ApiResult<UserProfileDTO>> UpdateSettingsAsync(UpdateUserSettingsDTO settings)
        {
            var result = await userApi.UpdateUserSettingsAsync(UserId, settings);
            if (result.IsSuccess)
            {
                ApplyProfile(result.Value);
            }
            return result;
        }

        /// <summary>Changes the current user's password.</summary>
        public Task<ApiResult> ChangePasswordAsync(ChangePasswordDTO password)
            => userApi.ChangePasswordAsync(UserId, password);

        /// <summary>
        /// Replaces the cached user with data from <paramref name="profile"/> and raises
        /// <see cref="OnChange"/> if anything shown in the layout actually changed.
        /// Profiles of other users are ignored.
        /// </summary>
        public void ApplyProfile(UserProfileDTO profile)
        {
            if (profile.Id != UserId)
            {
                return;
            }

            var updated = new UserMinDTO(
                profile.Id,
                profile.first_name,
                profile.last_name,
                profile.email,
                profile.role,
                profile.profile_image_url);

            // Records compare by value, so this skips no-op notifications.
            if (updated == currentUser)
            {
                return;
            }

            currentUser = updated;
            OnChange?.Invoke();
        }

        private async Task<UserMinDTO?> LoadCurrentUserAsync()
        {
            UserMinDTO? loaded;
            try
            {
                loaded = await userApi.getUserByIdAsync(UserId);
            }
            catch (HttpRequestException)
            {
                loaded = null;
            }

            // A profile update may have landed while this request was in flight; keep the newer data.
            currentUser ??= loaded;
            return currentUser;
        }
    }
}
