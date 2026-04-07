using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class CustomsClearanceConfiguration : IEntityTypeConfiguration<CustomsClearance>
    {
        public void Configure(EntityTypeBuilder<CustomsClearance> builder)
        {
            builder.HasOne(cc => cc.ImportRequest)
                .WithOne(ir => ir.CustomsClearance)
                .HasForeignKey<CustomsClearance>(cc => cc.ImportRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // Prevent multiple cascade paths through Users → CustomsDocuments
            builder.HasOne(cc => cc.HandledByOffice)
                .WithMany()
                .HasForeignKey(cc => cc.HandledByOfficeId)
                .OnDelete(DeleteBehavior.Restrict);   // ← was CASCADE (EF default)
        }
    }
}