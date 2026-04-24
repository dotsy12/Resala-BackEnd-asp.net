using BackEnd.Domain.Entities.Sponsorship;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackEnd.Infrastructure.Persistence.Configurations
{
    public class SponsorshipConfiguration : IEntityTypeConfiguration<Sponsorship>
    {
        public void Configure(EntityTypeBuilder<Sponsorship> builder)
        {
            builder.ToTable("Sponsorships");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ImagePath).HasMaxLength(1024);
            builder.Property(x => x.ImagePublicId).HasMaxLength(256);
            builder.Property(x => x.IconPath).HasMaxLength(256);

            builder.OwnsOne(x => x.FinancialGoal, m =>
            {
                m.Property(p => p.Amount).HasColumnName("Goal_Amount").HasPrecision(18, 2);
                m.Property(p => p.Currency).HasColumnName("Goal_Currency").HasMaxLength(10);
            });

            builder.OwnsOne(x => x.TotalCollected, m =>
            {
                m.Property(p => p.Amount).HasColumnName("Collected_Amount").HasPrecision(18, 2);
                m.Property(p => p.Currency).HasColumnName("Collected_Currency").HasMaxLength(10);
            });
            // أضف هذا السطر
            builder.Ignore(x => x.IsActive);
            builder.OwnsOne(x => x.Policy, p =>
            {
                p.Property(x => x.GracePeriodDays).HasColumnName("Policy_GracePeriodDays");
                p.Property(x => x.MaxDelayDays).HasColumnName("Policy_MaxDelayDays");
                p.Property(x => x.ReminderDaysBeforeDue).HasColumnName("Policy_ReminderDaysBeforeDue");
            });
        }
    }
}