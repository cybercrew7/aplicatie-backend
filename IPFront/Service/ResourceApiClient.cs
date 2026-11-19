using Shared.DTO;
using System.Net.Http.Json;

namespace IPFront.Service
{
    public class ResourceApiClient
    {
        private readonly HttpClient httpClient;

        public ResourceApiClient(HttpClient _client)
        {
            httpClient = _client;
        }

        public async Task<IEnumerable<ResourcesFullDTO>> GetAllResourcesAsync()
        {
            return await httpClient.GetFromJsonAsync<IEnumerable<ResourcesFullDTO>>($"api/Resources/getAll") ?? [];
        }

        public async Task<ResourcesFullDTO?> GetResourceByIdAsync(int id)
        {
            var response = await httpClient.GetAsync($"api/Resources/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ResourcesFullDTO>();
        }

        public async Task<IEnumerable<ResourcesMinimalDTO>> GetAvailableResourcesAsync()
        {
            return await httpClient.GetFromJsonAsync<IEnumerable<ResourcesMinimalDTO>>($"api/Resources/getAvailable") ?? [];
        }

        public async Task<ResourcesMinimalDTO?> CreateResourceAsync(ResourcesRoomCreateDTO resourceCreateDTO)
        {
            var response = await httpClient.PostAsJsonAsync("api/Resources/createroom", resourceCreateDTO);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ResourcesMinimalDTO>();
        }

        public async Task<ResourcesMinimalDTO?> CreateEquipmentAsync(ResourcesEquipmentCreateDTO resourceCreateDTO)
        {
            var response = await httpClient.PostAsJsonAsync("api/Resources/createEquipment", resourceCreateDTO);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ResourcesMinimalDTO>();
        }




    }
}
