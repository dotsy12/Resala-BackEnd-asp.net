using BackEnd.Domain.Entities.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackEnd.Infrastructure.Persistence.Configurations
{
    public class BranchAppointmentSlotConfiguration
        : IEntityTypeConfiguration<BranchAppointmentSlot>
    {
        public void Configure(EntityTypeBuilder<BranchAppointmentSlot> builder)
        {
            builder.ToTable("BranchAppointmentSlots");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.SlotDate).IsRequired();
            builder.Property(x => x.OpenFrom).IsRequired();
            builder.Property(x => x.OpenTo).IsRequired();
            builder.Property(x => x.MaxCapacity).IsRequired();
            builder.Property(x => x.BookedCount).IsRequired();
            builder.Property(x => x.IsActive).IsRequired();
            builder.Property(x => x.Notes).HasMaxLength(500);
        }
    }
}