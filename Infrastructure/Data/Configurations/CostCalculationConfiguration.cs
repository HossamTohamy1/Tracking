using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class CostCalculationConfiguration : IEntityTypeConfiguration<CostCalculation>
    {
        public void Configure(EntityTypeBuilder<CostCalculation> builder)
        {
            builder.Property(x => x.BaseShippingCost).HasPrecision(18, 2);
            builder.Property(x => x.CustomsDuty).HasPrecision(18, 2);
            builder.Property(x => x.TaxAmount).HasPrecision(18, 2);
            builder.Property(x => x.InsuranceCost).HasPrecision(18, 2);
            builder.Property(x => x.HandlingFee).HasPrecision(18, 2);
            builder.Property(x => x.OtherFees).HasPrecision(18, 2);
            builder.Property(x => x.DiscountAmount).HasPrecision(18, 2);
            builder.Property(x => x.TotalBeforeDiscount).HasPrecision(18, 2);
            builder.Property(x => x.FinalAmount).HasPrecision(18, 2);

            builder.HasOne(cc => cc.ImportRequest)
                .WithOne(ir => ir.CostCalculation)
                .HasForeignKey<CostCalculation>(cc => cc.ImportRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}