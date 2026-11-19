
using Shared.DTO;
using System.Net.Http.Json;



namespace IPFront.Service
{
    public class UserApiClient
    {
        private readonly HttpClient httpClient;

        public UserApiClient(HttpClient _httpClient)
        {
            httpClient = _httpClient;
        }

        public async Task<IEnumerable<UserMinDTO>> getUsersAsync()
        {
            return await httpClient.GetFromJsonAsync<IEnumerable<UserMinDTO>>($"api/User/getUser");

        }

        public async Task<UserMinDTO> getUserByIdAsync(int id)
        {
            return await httpClient.GetFromJsonAsync<UserMinDTO>($"api/User/getUserId/{id}");
        }

        public async Task<UserMinDTO> createUserAsync(UserRegisterDTO userRegisterDTO)
        {
            var response = await httpClient.PostAsJsonAsync("api/User/register", userRegisterDTO);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserMinDTO>();
        }

        public async Task<UserResponseAuthDTO> loginUserAsync(UserLoginDTO userLogin)
        {
            var response = await httpClient.PostAsJsonAsync("api/User/login", userLogin);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserResponseAuthDTO>();
            }
            else
            {
                // Handle error response, e.g., throw an exception or return null
                throw new Exception("Login failed");
            }
        }



    }
}
