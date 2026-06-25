using BackEnd.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackEnd.Infrastructure.Persistence.Configurations
{
    public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            builder.HasKey(c => c.Id);

            builder.HasOne(c => c.Donor)
                   .WithMany()
                   .HasForeignKey(c => c.DonorId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.Sponsorship)
                   .WithMany()
                   .HasForeignKey(c => c.SponsorshipId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(c => c.EmergencyCase)
                   .WithMany()
                   .HasForeignKey(c => c.EmergencyCaseId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.OwnsOne(c => c.DonationAmount, money =>
            {
                money.Property(m => m.Amount)
                     .HasColumnName("DonationAmount")
                     .HasPrecision(18, 2)
                     .IsRequired();

                money.Property(m => m.Currency)
                     .HasColumnName("DonationCurrency")
                     .HasMaxLength(10)
                     .IsRequired();
            });

            builder.Navigation(c => c.DonationAmount)
                   .IsRequired();
        }
    }
}
