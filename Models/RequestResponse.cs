namespace BloodBridge.Models
{
    public class RequestResponse
    {
        public int Id { get; set; }
        public int BloodRequestId { get; set; }
        public int DonorProfileId { get; set; }
        public DateTime ResponseDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Declined

        // Navigation
        public BloodRequest BloodRequest { get; set; } = null!;
        public DonorProfile DonorProfile { get; set; } = null!;
    }

    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Link { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ApplicationUser User { get; set; } = null!;
    }
}
