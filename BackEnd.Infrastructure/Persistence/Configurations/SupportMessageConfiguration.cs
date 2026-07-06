using BackEnd.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackEnd.Infrastructure.Persistence.Configurations
{
    public class SupportMessageConfiguration : IEntityTypeConfiguration<SupportMessage>
    {
        public void Configure(EntityTypeBuilder<SupportMessage> builder)
        {
            builder.ToTable("SupportMessages");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.MessageText)
                .IsRequired()
                .HasMaxLength(4000);

            builder.Property(x => x.SenderId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(x => x.SenderName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.ChatOwnerUserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
