using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaigonRide.Models
{
    public class Vehicle
    {
        [Key]
        public int VehicleID { get; set; }  // ← bỏ ?

        [Required(ErrorMessage = "Vehicle type is required")]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Available";

        [Required(ErrorMessage = "Price per minute is required")]
        public double PricePerMinute { get; set; }

        public int? StationID { get; set; }  // ← thêm ?

        [ForeignKey("StationID")]
        public virtual Station? Station { get; set; }

        public void UpdateStatus(string newStatus)
        {
            Status = newStatus;
        }

        public double CalculateRate()
        {
            return PricePerMinute;
        }
    }
}