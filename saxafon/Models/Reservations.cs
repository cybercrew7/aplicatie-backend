using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;


namespace saxafon.Models
{
    [Index(nameof(ResourceId), nameof(start_time), nameof(end_time))]
    public class Reservations
    {
        [Key]
        public int Id { get; set;  }
        public Resources Resource { get; set; } = null!;
        public int ResourceId { get; set; } 
        public int UserId { get; set; }
        public User user { get; set; } = null!;

        [MaxLength(300)]
        public string? purpose { get; set; }
        public Shared.ReservationStatus status_rezervare { get; set; }
        public DateTime start_time { get; set; } 
        public DateTime end_time { get; set; }
        public DateTime created_at { get; set; }
        public DateTime? canceled_at { get; set; }

        public int? expected_attendees { get; set; } // Validated against Rooms.capacity[cite: 4]

        // Approval / Rejection / Cancel audit
        public int? approved_by_user_id { get; set; }
        public User? approved_by_user { get; set; }
        public DateTime? reviewed_at { get; set; }

        [MaxLength(300)]
        public string? rejection_or_cancel_reason { get; set; }


    }
}
