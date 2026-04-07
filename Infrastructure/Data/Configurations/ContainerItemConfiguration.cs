using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class ContainerItemConfiguration : IEntityTypeConfiguration<ContainerItem>
    {
        public void Configure(EntityTypeBuilder<ContainerItem> builder)
        {
            builder.HasIndex(ci => ci.ImportRequestId).IsUnique();

            builder.HasOne(ci => ci.ImportRequest)
                .WithOne(ir => ir.ContainerItem)
                .HasForeignKey<ContainerItem>(ci => ci.ImportRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ci => ci.Container)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.ContainerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}