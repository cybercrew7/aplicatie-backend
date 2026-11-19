

namespace Shared.DTO
{
    public record ResourcesFullDTO(

            int Id,
            string resource_name,
            int? ParentId,
            Status status,
            string? type,
            bool is_approval_needed,
            DateTime created_at,
            DateTime updated_at,
            DateTime ?deleted_at,
            string ?equipment_model,
            string? description,
            string ?serial_number,
            string ?inventory_code,
            int ?capacity,
            string ?buildingId,
            string? image_url,
            int max_concurrent_bookings,
            int? min_booking_minutes,
            int? max_booking_minutes,
            int? max_bookings_per_user_per_week
        );


    public record ResourcesMinimalDTO(
            int Id,
            string resource_name,
            int? ParentId,
            Status status,
            bool? is_approval_needed,
            string? type,
            string? equipment_model,
            string? description,
            string? serial_number,
            string? inventory_code,
            int? capacity,
            string? buildingId,
            string? image_url
     );


  

    public record ResourcesRoomCreateDTO(

            string resource_name,
            Status status,
            bool is_approval_needed,
            string description,
            int capacity,
            string buildingId

    );

    
    public record ResourcesEquipmentCreateDTO(
          
            string resource_name,
            int ParentId,
            Status status,
            bool is_approval_needed,
            string type,
            string equipment_model,
            string description,
            string serial_number,
            string inventory_code
        );

}
