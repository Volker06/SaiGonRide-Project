namespace SaigonRide.Models
{
    public class Payment
    {
        public int PaymentID { get; set; }
        public int? RentalID { get; set; }
        public double Amount { get; set; }
        public string PaymentMethod { get; set; } = "";
        public string PaymentStatus { get; set; } = "Pending";
        public DateTime? PaymentDate { get; set; }

        // Navigation property
        public Rental? Rental { get; set; }
    }
}