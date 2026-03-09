using BackEnd.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackEnd.Infrastructure.Persistence.Configurations
{
    public class DonorConfiguration : IEntityTypeConfiguration<Donor>
    {
        public void Configure(EntityTypeBuilder<Donor> builder)
        {
            builder.ToTable("Donors");
            builder.HasKey(x => x.Id);

            builder.OwnsOne(x => x.FullName, n =>
            {
                n.Property(p => p.FirstName).HasColumnName("FirstName").HasMaxLength(100);
                n.Property(p => p.LastName).HasColumnName("LastName").HasMaxLength(100);
            });

            builder.OwnsOne(x => x.Email, e =>
            {
                e.Property(p => p.Value).HasColumnName("Email").HasMaxLength(256);
            });

            builder.OwnsOne(x => x.PhoneNumber, p =>
            {
                p.Property(x => x.Value).HasColumnName("PhoneNumber").HasMaxLength(20);
            });
        }
    }
}