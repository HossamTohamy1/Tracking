using Domain.Constants;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Seeders
{

    public class RoleSeeder
    {
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private readonly ILogger<RoleSeeder> _logger;

        public RoleSeeder(
            RoleManager<IdentityRole<Guid>> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration config,
            ILogger<RoleSeeder> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _config = config;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            // ── 1. Seed Roles ────────────────────────────────────────────
            foreach (var role in AppRoles.All)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole<Guid>(role));
                    _logger.LogInformation("Role '{Role}' created.", role);
                }
            }

            // ── 2. Seed Default Admin ────────────────────────────────────
            await SeedSystemUserAsync(
                section: "DefaultAdmin",
                role: AppRoles.Admin,
                defaultName: "System Admin");

            // ── 3. Seed Default Support ──────────────────────────────────
            await SeedSystemUserAsync(
                section: "DefaultSupport",
                role: AppRoles.Support,
                defaultName: "Support Agent");
        }

        // ─── Private ─────────────────────────────────────────────────────

        private async Task SeedSystemUserAsync(
            string section,
            string role,
            string defaultName)
        {
            var email = _config[$"{section}:Email"];
            var password = _config[$"{section}:Password"];
            var fullName = _config[$"{section}:FullName"] ?? defaultName;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning(
                    "Skipping seeding for role '{Role}': " +
                    "Missing {Section}:Email or {Section}:Password in configuration.",
                    role, section, section);
                return;
            }

            var existing = await _userManager.FindByEmailAsync(email);
            if (existing is not null)
            {
                _logger.LogInformation(
                    "System user '{Email}' ({Role}) already exists — skipping.", email, role);
                return;
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError(
                    "Failed to create system user '{Email}' ({Role}): {Errors}",
                    email, role, errors);
                return;
            }

            await _userManager.AddToRoleAsync(user, role);
            _logger.LogInformation(
                "System user '{Email}' created and assigned role '{Role}'.", email, role);
        }
    }
}