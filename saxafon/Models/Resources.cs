using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace saxafon.Models
{
   
    public class Resources
    {

        [Key]
        public int Id { get; set; }
        [MaxLength(50)]
        public string resource_name { get; set; } = null!;
        public int? ParentId { get; set; }
        [ForeignKey(nameof(ParentId))]
        public Resources? Parent { get; set; }
        public ICollection<Resources> Children { get; set; } = new List<Resources>();
        public Shared.Status status { get; set; }
        public bool is_approval_needed { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime? deleted_at { get; set; }

        [MaxLength(500)]
        public string? image_url { get; set; }

        public int max_concurrent_bookings { get; set; } = 1;
        public int? min_booking_minutes { get; set; }
        public int? max_booking_minutes { get; set; }
        public int? max_bookings_per_user_per_week { get; set; }
    }
}
