using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class TrackingHistoryConfiguration : IEntityTypeConfiguration<TrackingHistory>
    {
        public void Configure(EntityTypeBuilder<TrackingHistory> builder)
        {
            builder.HasOne(th => th.UpdatedBy)
                .WithMany()
                .HasForeignKey(th => th.UpdatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}