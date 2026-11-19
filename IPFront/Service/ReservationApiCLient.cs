using Shared.DTO;

using System.Net.Http.Json;

namespace IPFront.Service
{
    /// <summary>Typed HTTP client for the <c>api/Reservation</c> endpoints.</summary>
    public class ReservationApiClient
    {


        private readonly HttpClient _httpClient;

        /// <summary>Creates the client; the base address is configured in <c>Program.cs</c>.</summary>
        public ReservationApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>Returns all reservations.</summary>
        public async Task<IEnumerable<ReservationFullDTO>> getAllReservationAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<ReservationFullDTO>>("api/Reservation/getAll") ?? [];
        }


        /// <summary>Returns a reservation by id.</summary>
        public async Task<ReservationFullDTO?> getReservationByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<ReservationFullDTO>($"api/Reservation/{id}");
        }

        /// <summary>Returns the reservations of a resource.</summary>
        public async Task<IEnumerable<ReservationFullDTO>> getReservationByResourceIdAsync(int resourceId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<ReservationFullDTO>>($"api/Reservation/resource/{resourceId}") ?? [];
        }

        /// <summary>Returns all reservations of a user.</summary>
        public async Task<IEnumerable<ReservationFullDTO>> getAllReservationByUserIdAsync(int userId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<ReservationFullDTO>>($"api/Reservation/user/{userId}") ?? [];
        }

        
        /// <summary>Returns the current (not yet finished) reservations of a user.</summary>
        public async Task<IEnumerable<ReservationFullDTO>> getCurrentReservationByUserIdAsync(int userId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<ReservationFullDTO>>($"api/Reservation/current/{userId}") ?? [];
        }


        /// <summary>Cancels a reservation. Throws <see cref="HttpRequestException"/> with the API message on failure.</summary>
        public async Task<ReservationCancelResponseDTO?> CancelReservationAsync(ReservationCancelDTO reservation)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/Reservation/cancel", reservation);
            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<ReservationCancelResponseDTO>();
        }

        /// <summary>Creates a reservation. Throws <see cref="HttpRequestException"/> with the API message on failure.</summary>
        public async Task<ReservationFullDTO?> CreateReservationAsync(ReservationCreateDTO reservation)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/Reservation/create", reservation);
            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<ReservationFullDTO>();
        }


        /// <summary>Returns this week's reservations of a resource.</summary>
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
