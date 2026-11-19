using System.ComponentModel.DataAnnotations;

namespace saxafon.Models
{
    public class Rooms : Resources
    {

        
        public int? capacity { get; set; }
        [MaxLength(20)]
     
        public string? buildingId { get; set; }
      
    }
}
