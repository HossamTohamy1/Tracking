using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.HasOne(r => r.Reviewer)
                .WithMany(u => u.WrittenReviews)
                .HasForeignKey(r => r.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Office)
                .WithMany(u => u.ReceivedReviews)
                .HasForeignKey(r => r.OfficeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}