using BackEnd.Domain.Entities.Notification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackEnd.Infrastructure.Persistence.Configurations
{
    public class DeliveryAreaConfiguration : IEntityTypeConfiguration<DeliveryArea>
    {
        public void Configure(EntityTypeBuilder<DeliveryArea> builder)
        {
            builder.ToTable("DeliveryAreas");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Name).IsRequired().HasMaxLength(200);
            builder.Property(a => a.Governorate).IsRequired().HasMaxLength(100);
            builder.Property(a => a.City).IsRequired().HasMaxLength(100);
            
            builder.Property(a => a.IsActive).HasDefaultValue(true);
            builder.Property(a => a.IsDeleted).HasDefaultValue(false);

            builder.HasIndex(a => new { a.Name, a.Governorate, a.City }).IsUnique()
                .HasFilter("[IsDeleted] = 0");
        }
    }
}
