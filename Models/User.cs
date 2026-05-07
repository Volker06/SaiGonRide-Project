namespace SaigonRide.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string? Phone { get; set; }
        public string UserType { get; set; } = ""; // "Local", "Tourist", "Admin"
        public string? PaymentPreference { get; set; }
        public string? PassportNumber { get; set; }
        public string? Nationality { get; set; }
        public int AdminLevel { get; set; } = 0;
    }
}