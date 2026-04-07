using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Data.Configurations
{
    internal class ImportRequestConfiguration : IEntityTypeConfiguration<ImportRequest>
    {
        public void Configure(EntityTypeBuilder<ImportRequest> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.HasOne(ir => ir.User)
                .WithMany(u => u.MyImportRequests)
                .HasForeignKey(ir => ir.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ir => ir.AssignedOffice)
                .WithMany(u => u.AssignedRequests)
                .HasForeignKey(ir => ir.AssignedOfficeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}