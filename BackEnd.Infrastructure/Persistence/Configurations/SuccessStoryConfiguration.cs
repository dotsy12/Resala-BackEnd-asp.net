using BackEnd.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackEnd.Infrastructure.Persistence.Configurations
{
    public class SuccessStoryConfiguration : IEntityTypeConfiguration<SuccessStory>
    {
        public void Configure(EntityTypeBuilder<SuccessStory> builder)
        {
            builder.ToTable("SuccessStories");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(250);

            builder.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(4000);

            builder.Property(x => x.ImageUrl)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.ImagePublicId)
                .IsRequired()
                .HasMaxLength(500);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
