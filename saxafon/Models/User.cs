using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using Shared.DTO;

namespace saxafon.Models
{
 

    [Index(nameof(email),IsUnique =true)]
    [Index(nameof(user_name),IsUnique =true)]

    public class User
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(50)]
        public string first_name { get; set; } = null!;
        [MaxLength(50)]
        public string last_name { get; set; } = null!;
        [MaxLength(256)]
        public string email { get; set; } = null!;
        [MaxLength(50)]
        public string user_name { get; set; } = null!;
        [MaxLength(13)]
        public string? phone_number { get; set; }
        public Shared.Role role { get; set; }
        [MaxLength(256)]
        public string passwordHash { get; set; } = null!;

        [MaxLength(500)]
        public string? profile_image_url { get; set; }

        public DateTime? profile_image_updated_at { get; set; }

        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime last_time_active_at { get; set;  }
        public DateTime? deleted_at { get; set; }

       

    }
}
