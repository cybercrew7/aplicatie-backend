using System.ComponentModel.DataAnnotations;

namespace saxafon.Models
{
    public class Equipments : Resources
    {

        [MaxLength(50)]
        public string type { get; set; } = null!;
        [MaxLength(100)]
        public string? equipment_model { get; set; }
        [MaxLength(500)]
        public string? description { get; set; }
        [MaxLength(50)]
        public string? serial_number { get; set; }
        [MaxLength(30)]
        public string? inventory_code { get; set; }
    }
}
