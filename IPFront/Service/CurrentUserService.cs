using Microsoft.JSInterop;
using Shared.DTO;

namespace IPFront.Service
{
    public class CurrentUserService
    {
        private const string StorageKey = "unilab_current_user_id";

        private readonly UserApiClient userApi;
        private readonly IJSRuntime jsRuntime;
        private UserMinDTO? currentUser;
        private Task<UserMinDTO?>? loadTask;
        private bool storageChecked;

        public CurrentUserService(UserApiClient _userApi, IJSRuntime _jsRuntime)
        {
            userApi = _userApi;
            jsRuntime = _jsRuntime;
        }

        public event Action? OnChange;

        public int UserId { get; private set; } = 0;

        public UserMinDTO? CurrentUser => currentUser;

        public async Task<UserMinDTO?> GetCurrentUserAsync()
        {
            if (!storageChecked)
            {
                storageChecked = true;
                try
                {
                    var stored = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
                    if (int.TryParse(stored, out var parsedId) && parsedId > 0)
                    {
                        UserId = parsedId;
                    }
                }
                catch
                {
                    // Ignore JS interop errors during init
                }
            }

            if (UserId <= 0)
            {
                return null;
            }

            if (currentUser is not null && currentUser.Id == UserId)
            {
                return currentUser;
            }

            if (loadTask is null || loadTask.IsCompleted)
            {
                loadTask = LoadCurrentUserAsync();
            }
            return await loadTask;
        }

        public async Task<UserMinDTO?> SetCurrentUserAsync(int userId)
        {
            UserId = userId;
            currentUser = null;
            loadTask = null;
            storageChecked = true;

            try
            {
                await jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, userId.ToString());
            }
            catch
            {
                // Ignore storage errors
            }

            var loaded = await GetCurrentUserAsync();
            OnChange?.Invoke();
            return loaded;
        }

        public async Task LogoutAsync()
        {
            UserId = 0;
            currentUser = null;
            loadTask = null;

            try
            {
                await jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKey);
            }
            catch
            {
                // Ignore storage errors
            }

            OnChange?.Invoke();
        }

        public async Task<ApiResult<UserProfileDTO>> GetProfileAsync()
        {
            var result = await userApi.GetUserProfileAsync(UserId);
            if (result.IsSuccess)
            {
                ApplyProfile(result.Value);
            }
            return result;
        }

        public async Task<ApiResult<UserProfileDTO>> UpdateProfileAsync(UpdateUserProfileDTO profile)
        {
            var result = await userApi.UpdateUserProfileAsync(UserId, profile);
            if (result.IsSuccess)
            {
                ApplyProfile(result.Value);
            }
            return result;
        }

        public async Task<ApiResult<UserProfileDTO>> UpdateSettingsAsync(UpdateUserSettingsDTO settings)
        {
            var result = await userApi.UpdateUserSettingsAsync(UserId, settings);
            if (result.IsSuccess)
            {
                ApplyProfile(result.Value);
            }
            return result;
        }

        public Task<ApiResult> ChangePasswordAsync(ChangePasswordDTO password)
            => userApi.ChangePasswordAsync(UserId, password);

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

            currentUser ??= loaded;
            OnChange?.Invoke();
            return currentUser;
        }
    }
}