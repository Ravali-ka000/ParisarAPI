using System.ComponentModel.DataAnnotations;

namespace ParisarAPI.Models
{
    public class Location : BaseEntity

    {
        [Required]
        public string Name { get; set; }    
    }
}
