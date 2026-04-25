using BackEnd.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackEnd.Infrastructure.Persistence.Configurations
{
    public class StaffMemberConfiguration : IEntityTypeConfiguration<StaffMember>
    {
        public void Configure(EntityTypeBuilder<StaffMember> builder)
        {
            builder.ToTable("StaffMembers");
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

            builder.OwnsOne(x => x.Phone, p =>
            {
                p.Property(x => x.Value).HasColumnName("PhoneNumber").HasMaxLength(20);
            });

            builder.HasOne(x => x.User)
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}