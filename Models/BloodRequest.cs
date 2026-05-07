namespace BloodBridge.Models
{
    public enum RequestStatus
    {
        Open,
        InProgress,
        Fulfilled,
        Expired
    }

    public class BloodRequest
    {
        public int Id { get; set; }
        public string RequesterId { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public string HospitalName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public RequestStatus Status { get; set; } = RequestStatus.Open;
        public bool IsUrgent { get; set; } = false;
        public DateTime Deadline { get; set; }
        public string? AdditionalInfo { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ApplicationUser Requester { get; set; } = null!;
        public ICollection<RequestResponse> Responses { get; set; } = new List<RequestResponse>();
    }
}
