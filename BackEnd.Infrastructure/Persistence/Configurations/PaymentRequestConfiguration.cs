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

            // BranchPaymentDetails — Value Object → Owned
            builder.OwnsOne(x => x.BranchDetails, b =>
            {
                b.Property(p => p.DonorName).HasColumnName("Branch_DonorName").HasMaxLength(200);
                b.Property(p => p.Address).HasColumnName("Branch_Address").HasMaxLength(500);
                b.Property(p => p.ContactNumber).HasColumnName("Branch_ContactNumber").HasMaxLength(20);
                b.Property(p => p.ScheduledDate).HasColumnName("Branch_ScheduledDate");
            });

            // RepresentativeDetails — Value Object → Owned
            builder.OwnsOne(x => x.RepresentativeInfo, r =>
            {
                r.Property(p => p.DeliveryAreaId).HasColumnName("Rep_DeliveryAreaId");
                r.Property(p => p.DeliveryAreaName).HasColumnName("Rep_DeliveryAreaName").HasMaxLength(200);
                r.Property(p => p.Notes).HasColumnName("Rep_Notes").HasMaxLength(500);
            });

            // Money — Value Object → Owned
            builder.OwnsOne(x => x.Amount, m =>
            {
                m.Property(p => p.Amount).HasColumnName("Amount").HasPrecision(18, 2);
                m.Property(p => p.Currency).HasColumnName("Currency").HasMaxLength(10);
            });
        }
    }
}