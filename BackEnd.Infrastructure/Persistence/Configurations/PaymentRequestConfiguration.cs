using BackEnd.Domain.Entities.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackEnd.Infrastructure.Persistence.Configurations
{
    public class PaymentRequestConfiguration : IEntityTypeConfiguration<PaymentRequest>
    {
        public void Configure(EntityTypeBuilder<PaymentRequest> builder)
        {
            builder.ToTable("PaymentRequests");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.SenderPhoneNumber).HasMaxLength(20);
            builder.Property(x => x.ReceiptImageUrl).HasMaxLength(1024);
            builder.Property(x => x.ReceiptImagePublicId).HasMaxLength(256);

            builder.OwnsOne(x => x.BranchDetails, b =>
            {
                b.Property(p => p.DonorName).HasColumnName("Branch_DonorName").HasMaxLength(200);
                // ✅ أزلنا Branch_Address
                b.Property(p => p.ContactNumber).HasColumnName("Branch_ContactNumber").HasMaxLength(20);
                b.Property(p => p.ScheduledDate).HasColumnName("Branch_ScheduledDate");
                b.Property(p => p.SlotId).HasColumnName("Branch_SlotId");  // ✅ جديد
            });

            builder.OwnsOne(x => x.RepresentativeInfo, r =>
            {
                r.Property(p => p.DeliveryAreaId).HasColumnName("Rep_DeliveryAreaId");
                r.Property(p => p.DeliveryAreaName).HasColumnName("Rep_DeliveryAreaName").HasMaxLength(200);
                r.Property(p => p.ContactName).HasColumnName("Rep_ContactName").HasMaxLength(200);   // ✅ جديد
                r.Property(p => p.ContactPhone).HasColumnName("Rep_ContactPhone").HasMaxLength(20);  // ✅ جديد
                r.Property(p => p.Address).HasColumnName("Rep_Address").HasMaxLength(500);           // ✅ جديد
                r.Property(p => p.Notes).HasColumnName("Rep_Notes").HasMaxLength(500);
            });

            builder.OwnsOne(x => x.Amount, m =>
            {
                m.Property(p => p.Amount).HasColumnName("Amount").HasPrecision(18, 2);
                m.Property(p => p.Currency).HasColumnName("Currency").HasMaxLength(10);
            });

            builder.HasOne(x => x.Subscription)
                   .WithMany()
                   .HasForeignKey(x => x.SubscriptionId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.EmergencyCase)
                   .WithMany()
                   .HasForeignKey(x => x.EmergencyCaseId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Donor)
                   .WithMany()
                   .HasForeignKey(x => x.DonorId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}