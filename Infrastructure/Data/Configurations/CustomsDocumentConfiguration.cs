using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class CustomsDocumentConfiguration : IEntityTypeConfiguration<CustomsDocument>
    {
        public void Configure(EntityTypeBuilder<CustomsDocument> builder)
        {
            // UploadedBy: explicit mapping to suppress the shadow FK (ApplicationUserId)
            builder.HasOne(cd => cd.UploadedBy)
                .WithMany(u => u.UploadedDocuments)   // ← tell EF which nav to use
                .HasForeignKey(cd => cd.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // VerifiedBy: nullable, no cascade
            builder.HasOne(cd => cd.VerifiedBy)
                .WithMany()
                .HasForeignKey(cd => cd.VerifiedByUserId)
                .OnDelete(DeleteBehavior.NoAction);     // works because FK is nullable
        }
    }
}