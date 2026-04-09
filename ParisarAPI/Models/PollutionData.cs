using System.ComponentModel.DataAnnotations.Schema;

namespace ParisarAPI.Models
{
    public class PollutionData : BaseEntity
    {

        public DateTime Date { get; set; }
        [ForeignKey("Location")]
        public int LocationId { get; set; }
        [ForeignKey(nameof(LocationId))]
        public Location Location { get; set; }
        public double PM10 { get; set; }
        public double PM25 { get; set; }

    }
}
