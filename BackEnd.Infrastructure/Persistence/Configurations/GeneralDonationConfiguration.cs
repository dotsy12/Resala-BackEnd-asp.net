using BackEnd.Domain.Entities.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackEnd.Infrastructure.Persistence.Configurations
{
    public class GeneralDonationConfiguration : IEntityTypeConfiguration<GeneralDonation>
    {
        public void Configure(EntityTypeBuilder<GeneralDonation> builder)
        {
            builder.ToTable("GeneralDonations");
            builder.HasKey(x => x.Id);

            builder.OwnsOne(x => x.Amount, m =>
            {
                m.Property(p => p.Amount).HasColumnName("Amount").HasPrecision(18, 2);
                m.Property(p => p.Currency).HasColumnName("Currency").HasMaxLength(10);
            });
        }
    }
}