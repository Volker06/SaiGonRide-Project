using System.ComponentModel.DataAnnotations;

namespace SaigonRide.Models
{
    public class Station
    {
        [Key]
        public int StationID { get; set; }

        [Required(ErrorMessage = "Station name is required")]
        [StringLength(100)]
        public string StationName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required")]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, 1000, ErrorMessage = "Capacity must be between 1 and 1000")]
        public int Capacity { get; set; }

        public int CurrentInventory { get; set; } = 0;

        // Navigation Property
        public virtual ICollection<Vehicle> Vehicles { get; set; }
            = new List<Vehicle>();

        public bool CheckAvailability()
        {
            return CurrentInventory > 0;
        }

        public void UpdateInventory(int change)
        {
            CurrentInventory += change;

            if (CurrentInventory < 0)
                CurrentInventory = 0;
        }
    }
}