namespace BloodBridge.Models
{
    public class DonorProfile
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        public DateTime? LastDonationDate { get; set; }
        public bool IsAvailable { get; set; } = true;
        public int TotalDonations { get; set; } = 0;

        // Navigation
        public ApplicationUser User { get; set; } = null!;
        public ICollection<DonationHistory> DonationHistories { get; set; } = new List<DonationHistory>();
        public ICollection<RequestResponse> RequestResponses { get; set; } = new List<RequestResponse>();

        // Computed
        public bool IsEligibleToDonate =>
            LastDonationDate == null || (DateTime.UtcNow - LastDonationDate.Value).TotalDays >= 56;

        public DateTime? EligibleAgainDate =>
            LastDonationDate?.AddDays(56);
    }
}
