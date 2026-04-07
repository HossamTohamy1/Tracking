using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class TrackingConfiguration : IEntityTypeConfiguration<Tracking>
    {
        public void Configure(EntityTypeBuilder<Tracking> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.HasOne(t => t.ImportRequest)
                .WithOne(ir => ir.Tracking)
                .HasForeignKey<Tracking>(t => t.ImportRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}