
namespace Shared.DTO
{

    public record ReservationFullDTO(
        int id,
        int resourceId,
        int userId,
        string? purpose,
        ReservationStatus status_rezervare,
        DateTime start_time,
        DateTime end_time,
        DateTime created_at,
        DateTime? canceled_at,
        int? expected_attendees,
        DateTime? reviewed_at,
        string? rejection_or_cancel_reason
    );

    public record ReservationViewDTO(
        int id,
        int resourceId,
        DateTime start_time,
        DateTime end_time
        
        );

    public record ReservationCancelDTO(
        int id,
        int resourceId,
        int userId
    );

    public record ReservationCancelResponseDTO(
        int id,
        DateTime canceled_at

    );
   

    public record ReservationCreateDTO(
        int resourceId,
        int userId,
        string ? prupose,
        DateTime start_time,
        DateTime end_time,
        int? expected_attendees
    );

    public record ReservationUpdateDTO(
        int id,
        int resourceId,
        int userId,
        string? purpose,
        ReservationStatus status_rezervare,
        DateTime start_time,
        DateTime end_time
    );

    public record ReservationTimeFrameDTO(
        int id, 
        int resorceId,
        ReservationStatus ReservationStatus,
        DateTime start_time,
        DateTime end_time
    );


   



}
