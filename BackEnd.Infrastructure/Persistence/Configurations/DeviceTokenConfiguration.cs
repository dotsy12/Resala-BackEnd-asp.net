using BackEnd.Domain.Entities.Notification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackEnd.Infrastructure.Persistence.Configurations
{
    public class DeviceTokenConfiguration : IEntityTypeConfiguration<DeviceToken>
    {
        public void Configure(EntityTypeBuilder<DeviceToken> builder)
        {
            builder.ToTable("DeviceTokens");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Token).IsRequired().HasMaxLength(1000);
            builder.Property(x => x.DeviceType).HasMaxLength(50);

            builder.HasIndex(x => x.Token).IsUnique();

            builder.HasOne(x => x.Donor)
                   .WithMany()
                   .HasForeignKey(x => x.DonorId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
