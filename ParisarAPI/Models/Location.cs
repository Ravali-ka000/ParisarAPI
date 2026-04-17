using System.ComponentModel.DataAnnotations;

namespace ParisarAPI.Models
{
    public class Location : BaseEntity

    {
        [Required]
        public string Name { get; set; }
        [Required]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public double? Latitude { get; set; }

        [Required]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public double? Longitude { get; set; }
        public string? Fileurl { get; set; }
    }
}
