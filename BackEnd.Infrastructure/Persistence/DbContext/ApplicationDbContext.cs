using BackEnd.Domain.Entities.EmergencyCase;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Domain.Entities.Notification;
using BackEnd.Domain.Entities.Payment;
using BackEnd.Domain.Entities.Sponsorship;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Infrastructure.Persistence.DbContext
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {


        }
       
        public DbSet<Donor> Donors { get; set; }
        public DbSet<StaffMember> StaffMembers { get; set; }
        public DbSet<OtpRecord> OtpRecords { get; set; }
        public DbSet<Sponsorship> Sponsorships { get; set; }
        public DbSet<SponsorshipSubscription> SponsorshipSubscriptions { get; set; }
        public DbSet<PaymentRequest> PaymentRequests { get; set; }
        public DbSet<EmergencyCase> EmergencyCases { get; set; }
        public DbSet<GeneralDonation> GeneralDonations { get; set; }
        public DbSet<InKindDonation> InKindDonations { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<DeliveryArea> DeliveryAreas { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            var adminRoleId = "admin-role-id";
            var receptionRoleId = "reception-role-id";
            var donorRoleId = "donor-role-id";
            var adminUserId = "admin-user-id";

            builder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = adminRoleId, Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = "1" },
                new IdentityRole { Id = receptionRoleId, Name = "Reception", NormalizedName = "RECEPTION", ConcurrencyStamp = "2" },
                new IdentityRole { Id = donorRoleId, Name = "Donor", NormalizedName = "DONOR", ConcurrencyStamp = "3" }
            );

            builder.Entity<ApplicationUser>().HasData(new ApplicationUser
            {
                Id = adminUserId,
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@resala.org",
                NormalizedEmail = "ADMIN@RESALA.ORG",
                EmailConfirmed = true,
                IsActive = true,
                FirstName = "Admin",
                LastName = "Resala",
                PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(null!, "Admin@1234"), // ✅
                SecurityStamp = "admin-security",
                ConcurrencyStamp = "admin-concurrency",
                CreatedOn = new DateTime(2026, 1, 1)
            });

            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string> { UserId = adminUserId, RoleId = adminRoleId }
            );
            builder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.HasOne(r => r.User)
                      .WithMany()           // ✅ بدون navigation من ApplicationUser
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

    }
}
