using BackEnd.Domain.Entities.Sponsorship;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackEnd.Infrastructure.Persistence.Configurations
{
    public class SponsorshipSubscriptionConfiguration
        : IEntityTypeConfiguration<SponsorshipSubscription>
    {
        public void Configure(EntityTypeBuilder<SponsorshipSubscription> builder)
        {
            builder.ToTable("SponsorshipSubscriptions");
            builder.HasKey(x => x.Id);

            builder.OwnsOne(x => x.Amount, m =>
            {
                m.Property(p => p.Amount).HasColumnName("Amount").HasPrecision(18, 2);
                m.Property(p => p.Currency).HasColumnName("Currency").HasMaxLength(10);
            });
        }
    }
}