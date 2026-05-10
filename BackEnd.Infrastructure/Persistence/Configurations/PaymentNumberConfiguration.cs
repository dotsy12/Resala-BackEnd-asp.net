using BackEnd.Domain.Entities.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackEnd.Infrastructure.Persistence.Configurations
{
    public class PaymentNumberConfiguration : IEntityTypeConfiguration<PaymentNumber>
    {
        public void Configure(EntityTypeBuilder<PaymentNumber> builder)
        {
            builder.ToTable("PaymentNumbers");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.Number)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(x => x.Type).IsUnique();

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
