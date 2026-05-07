using System.ComponentModel.DataAnnotations;

namespace BloodBridge.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Register As")]
        public string Role { get; set; } = "Donor";
    }

    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }

    public class DonorProfileViewModel
    {
        [Required]
        [Display(Name = "Blood Type")]
        public string BloodType { get; set; } = string.Empty;

        [Display(Name = "Last Donation Date")]
        [DataType(DataType.Date)]
        public DateTime? LastDonationDate { get; set; }

        [Display(Name = "Available to Donate")]
        public bool IsAvailable { get; set; } = true;
    }

    public class DonationHistoryViewModel
    {
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Donation Date")]
        public DateTime DonationDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Hospital Name")]
        public string HospitalName { get; set; } = string.Empty;

        [Range(1, 10)]
        public int Units { get; set; } = 1;

        public string? Notes { get; set; }
    }

    public class BloodRequestViewModel
    {
        [Required]
        [Display(Name = "Blood Type")]
        public string BloodType { get; set; } = string.Empty;

        [Required]
        [Range(1, 50)]
        public int Quantity { get; set; } = 1;

        [Required]
        [Display(Name = "Hospital Name")]
        public string HospitalName { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime Deadline { get; set; } = DateTime.Today.AddDays(3);

        [Display(Name = "Mark as Urgent")]
        public bool IsUrgent { get; set; }

        [Display(Name = "Additional Information")]
        public string? AdditionalInfo { get; set; }
    }

    public class DonorSearchViewModel
    {
        public string? BloodType { get; set; }
        public string? City { get; set; }
        public bool OnlyAvailable { get; set; } = true;
        public List<DonorResultViewModel> Results { get; set; } = new();
    }

    public class DonorResultViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public bool IsEligible { get; set; }
        public int TotalDonations { get; set; }
    }

    public class AdminDashboardViewModel
    {
        public int TotalDonors { get; set; }
        public int TotalRequests { get; set; }
        public int OpenRequests { get; set; }
        public int FulfilledRequests { get; set; }
        public string MostNeededBloodType { get; set; } = string.Empty;
        public int TotalUsers { get; set; }
    }
}
