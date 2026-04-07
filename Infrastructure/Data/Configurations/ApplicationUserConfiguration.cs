using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.ToTable("Users");
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.HasMany(u => u.UploadedDocuments)
                .WithOne(cd => cd.UploadedBy)
                .HasForeignKey(cd => cd.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}