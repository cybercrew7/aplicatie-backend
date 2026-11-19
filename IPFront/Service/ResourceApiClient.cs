using Shared.DTO;
using System.Net.Http.Json;

namespace IPFront.Service
{
    /// <summary>Typed HTTP client for the <c>api/Resources</c> endpoints.</summary>
    public class ResourceApiClient
    {
        private readonly HttpClient httpClient;

        /// <summary>Creates the client; the base address is configured in <c>Program.cs</c>.</summary>
        public ResourceApiClient(HttpClient _client)
        {
            httpClient = _client;
        }

        /// <summary>Returns all resources.</summary>
        public async Task<IEnumerable<ResourcesFullDTO>> GetAllResourcesAsync()
        {
            return await httpClient.GetFromJsonAsync<IEnumerable<ResourcesFullDTO>>($"api/Resources/getAll") ?? [];
        }

        /// <summary>Returns a resource by id, or null if it does not exist.</summary>
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

        /// <summary>Returns the resources that are currently available.</summary>
        public async Task<IEnumerable<ResourcesMinimalDTO>> GetAvailableResourcesAsync()
        {
            return await httpClient.GetFromJsonAsync<IEnumerable<ResourcesMinimalDTO>>($"api/Resources/getAvailable") ?? [];
        }

        /// <summary>Creates a room.</summary>
        public async Task<ResourcesMinimalDTO?> CreateResourceAsync(ResourcesRoomCreateDTO resourceCreateDTO)
        {
            var response = await httpClient.PostAsJsonAsync("api/Resources/createroom", resourceCreateDTO);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ResourcesMinimalDTO>();
        }

        /// <summary>Creates an equipment item.</summary>
        public async Task<ResourcesMinimalDTO?> CreateEquipmentAsync(ResourcesEquipmentCreateDTO resourceCreateDTO)
        {
            var response = await httpClient.PostAsJsonAsync("api/Resources/createEquipment", resourceCreateDTO);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ResourcesMinimalDTO>();
        }




    }
}
