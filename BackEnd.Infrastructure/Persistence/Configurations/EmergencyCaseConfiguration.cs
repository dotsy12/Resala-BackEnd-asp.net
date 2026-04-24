using BackEnd.Domain.Entities.EmergencyCase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Infrastructure.Persistence.Configurations
{
    public class EmergencyCaseConfiguration : IEntityTypeConfiguration<EmergencyCase>
    {
        public void Configure(EntityTypeBuilder<EmergencyCase> builder)
        {
            builder.Property(e => e.ImagePath).HasMaxLength(1024);
            builder.Property(e => e.ImagePublicId).HasMaxLength(256);

            builder.OwnsOne(e => e.RequiredAmount, money =>
            {
                money.Property(m => m.Amount)
                     .HasColumnName("RequiredAmount")
                     .HasPrecision(18, 2)
                     .IsRequired();

                money.Property(m => m.Currency)
                     .HasColumnName("RequiredCurrency")
                     .HasMaxLength(10)
                     .IsRequired();
            });

            builder.Navigation(e => e.RequiredAmount)
                   .IsRequired();

            // -------------------------

            builder.OwnsOne(e => e.CollectedAmount, money =>
            {
                money.Property(m => m.Amount)
                     .HasColumnName("CollectedAmount")
                     .HasPrecision(18, 2)
                     .IsRequired();

                money.Property(m => m.Currency)
                     .HasColumnName("CollectedCurrency")
                     .HasMaxLength(10)
                     .IsRequired();
            });

            builder.Navigation(e => e.CollectedAmount)
                   .IsRequired();
        }
    }
}

