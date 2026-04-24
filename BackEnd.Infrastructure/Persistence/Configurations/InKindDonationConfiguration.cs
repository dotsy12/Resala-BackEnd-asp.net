// Infrastructure/Persistence/Configurations/InKindDonationConfiguration.cs
using BackEnd.Domain.Entities.Payment;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Infrastructure.Persistence.Configurations   // ✅ أُضيف namespace
{
    public class InKindDonationConfiguration : IEntityTypeConfiguration<InKindDonation>
    {
        public void Configure(EntityTypeBuilder<InKindDonation> builder)
        {
            builder.ToTable("InKindDonations");

            // اربط الـ Navigation بالـ FK الصح
            builder.HasOne(x => x.RecordedBy)
                   .WithMany()
                   .HasForeignKey(x => x.RecordedByStaffId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}