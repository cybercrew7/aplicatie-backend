using Shared.DTO;

namespace IPFront.Service
{
    /// <summary>
    /// Holds the identity of the user currently using the app.
    /// There is no login yet, so a development user id is used.
    /// TODO: set UserId from the login response once authentication is implemented.
    /// </summary>
    public class CurrentUserService
    {
        private readonly UserApiClient userApi;
        private UserMinDTO? currentUser;

        public CurrentUserService(UserApiClient _userApi)
        {
            userApi = _userApi;
        }

        public int UserId { get; private set; } = 1;

        public async Task<UserMinDTO?> GetCurrentUserAsync()
        {
            if (currentUser == null)
            {
                try
                {
                    currentUser = await userApi.getUserByIdAsync(UserId);
                }
                catch (HttpRequestException)
                {
                    currentUser = null;
                }
            }
            return currentUser;
        }
    }
}
