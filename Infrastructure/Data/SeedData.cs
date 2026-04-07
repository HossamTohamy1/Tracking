using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using Domain.Models;

namespace Logistics.Core.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(
            RoleManager<IdentityRole<Guid>> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            // ── إنشاء الـ Roles ────────────────────────────────────────
            string[] roles = { "Admin", "Office", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }

            // ── إنشاء Admin افتراضي ────────────────────────────────────
            const string adminEmail = "admin@logistics.com";

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "System Administrator",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(admin, "Admin@123456");

                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
