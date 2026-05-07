namespace SaigonRide.Models
{
    public class Rental
    {
        public int RentalID { get; set; }
        public int? UserID { get; set; }
        public int? VehicleID { get; set; }
        public int? StartStationID { get; set; }
        public int? ReturnStationID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double? TotalFare { get; set; }
        public double Discount { get; set; } = 0;
        public string Status { get; set; } = "Active";

        // Navigation properties
        public User? User { get; set; }
        public Vehicle? Vehicle { get; set; }
        public Station? StartStation { get; set; }
        public Station? ReturnStation { get; set; }

        // Pricing logic
        public double CalculateFare()
        {
            if (EndTime == null || Vehicle == null) return 0;
            double minutes = (EndTime.Value - StartTime).TotalMinutes;
            TotalFare = minutes * Vehicle.PricePerMinute;
            return TotalFare.Value;
        }

        public double ApplyDiscount(bool hasLocationDiscount)
        {
            if (hasLocationDiscount && TotalFare.HasValue)
            {
                Discount = TotalFare.Value * 0.15;
                TotalFare = TotalFare.Value - Discount;
            }
            return TotalFare ?? 0;
        }
    }
}