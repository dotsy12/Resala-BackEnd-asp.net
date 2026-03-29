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
            builder.OwnsOne(e => e.RequiredAmount, money =>
            {
                money.Property(m => m.Amount)
                     .HasColumnName("RequiredAmount");

                money.Property(m => m.Currency)
                     .HasColumnName("RequiredCurrency");
            });

            builder.OwnsOne(e => e.CollectedAmount, money =>
            {
                money.Property(m => m.Amount)
                     .HasColumnName("CollectedAmount");

                money.Property(m => m.Currency)
                     .HasColumnName("CollectedCurrency");
            });
        }
    }
}
