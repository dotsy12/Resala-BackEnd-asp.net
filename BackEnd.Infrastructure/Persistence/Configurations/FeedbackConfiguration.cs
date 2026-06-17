using BackEnd.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackEnd.Infrastructure.Persistence.Configurations
{
    public class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
    {
        public void Configure(EntityTypeBuilder<Feedback> builder)
        {
            builder.ToTable("Feedbacks");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Subject)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Message)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(x => x.Type)
                .HasConversion<int>();

            builder.Property(x => x.Status)
                .HasConversion<int>();

            builder.HasOne(x => x.Donor)
                .WithMany()
                .HasForeignKey(x => x.DonorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
