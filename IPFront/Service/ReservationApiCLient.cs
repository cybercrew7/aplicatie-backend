using Shared.DTO;

using System.Net.Http.Json;

namespace IPFront.Service
{
    public class ReservationApiClient
    {


        private readonly HttpClient _httpClient;

        public ReservationApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<ReservationFullDTO>> getAllReservationAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<ReservationFullDTO>>("api/Reservation/getAll") ?? [];
        }


        public async Task<ReservationFullDTO?> getReservationByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<ReservationFullDTO>($"api/Reservation/{id}");
        }

        public async Task<IEnumerable<ReservationFullDTO>> getReservationByResourceIdAsync(int resourceId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<ReservationFullDTO>>($"api/Reservation/resource/{resourceId}") ?? [];
        }

        public async Task<IEnumerable<ReservationFullDTO>> getAllReservationByUserIdAsync(int userId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<ReservationFullDTO>>($"api/Reservation/user/{userId}") ?? [];
        }

        
        public async Task<IEnumerable<ReservationFullDTO>> getCurrentReservationByUserIdAsync(int userId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<ReservationFullDTO>>($"api/Reservation/current/{userId}") ?? [];
        }


        public async Task<ReservationCancelResponseDTO?> CancelReservationAsync(ReservationCancelDTO reservation)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/Reservation/cancel", reservation);
            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<ReservationCancelResponseDTO>();
        }

        public async Task<ReservationFullDTO?> CreateReservationAsync(ReservationCreateDTO reservation)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/Reservation/create", reservation);
            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<ReservationFullDTO>();
        }


        public async Task<IEnumerable<ReservationViewDTO>> ShowCurrentWeekReservationsAsync(int resourceId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<ReservationViewDTO>>($"api/Reservation/thisWeek?resourceId={resourceId}") ?? [];
        }

        // The API returns the validation message as plain text for 400/404/409 responses.
        private static async Task EnsureSuccessAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var message = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(message) || message.TrimStart().StartsWith('{'))
            {
                message = $"Request failed ({(int)response.StatusCode}).";
            }
            throw new HttpRequestException(message, null, response.StatusCode);
        }


    }
}
