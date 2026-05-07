using BloodBridge.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BloodBridge.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<DonorProfile> DonorProfiles { get; set; }
        public DbSet<DonationHistory> DonationHistories { get; set; }
        public DbSet<BloodRequest> BloodRequests { get; set; }
        public DbSet<RequestResponse> RequestResponses { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // DonorProfile -> User (one-to-one)
            builder.Entity<DonorProfile>()
                .HasOne(d => d.User)
                .WithOne(u => u.DonorProfile)
                .HasForeignKey<DonorProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // DonationHistory -> DonorProfile
            builder.Entity<DonationHistory>()
                .HasOne(dh => dh.DonorProfile)
                .WithMany(d => d.DonationHistories)
                .HasForeignKey(dh => dh.DonorProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // BloodRequest -> User (requester)
            builder.Entity<BloodRequest>()
                .HasOne(r => r.Requester)
                .WithMany(u => u.BloodRequests)
                .HasForeignKey(r => r.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            // RequestResponse -> BloodRequest
            builder.Entity<RequestResponse>()
                .HasOne(rr => rr.BloodRequest)
                .WithMany(r => r.Responses)
                .HasForeignKey(rr => rr.BloodRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // RequestResponse -> DonorProfile
            builder.Entity<RequestResponse>()
                .HasOne(rr => rr.DonorProfile)
                .WithMany(d => d.RequestResponses)
                .HasForeignKey(rr => rr.DonorProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            // Notification -> User
            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
