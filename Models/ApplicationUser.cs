using Microsoft.AspNetCore.Identity;

namespace BloodBridge.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Donor, Patient, Hospital
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public DonorProfile? DonorProfile { get; set; }
        public ICollection<BloodRequest> BloodRequests { get; set; } = new List<BloodRequest>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
