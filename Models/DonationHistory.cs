namespace BloodBridge.Models
{
    public class DonationHistory
    {
        public int Id { get; set; }
        public int DonorProfileId { get; set; }
        public DateTime DonationDate { get; set; }
        public string HospitalName { get; set; } = string.Empty;
        public int Units { get; set; } = 1;
        public string? Notes { get; set; }

        // Navigation
        public DonorProfile DonorProfile { get; set; } = null!;
    }
}
