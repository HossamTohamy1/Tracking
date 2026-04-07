
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // ── DbSets ────────────────────────────────────────────────────
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ImportRequest> ImportRequests { get; set; }
        public DbSet<Tracking> Trackings { get; set; }
        public DbSet<TrackingHistory> TrackingHistories { get; set; }
        public DbSet<Container> Containers { get; set; }
        public DbSet<ContainerItem> ContainerItems { get; set; }
        public DbSet<CostCalculation> CostCalculations { get; set; }
        public DbSet<CustomsClearance> CustomsClearances { get; set; }
        public DbSet<CustomsDocument> CustomsDocuments { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ExportProduct> ExportProducts { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}