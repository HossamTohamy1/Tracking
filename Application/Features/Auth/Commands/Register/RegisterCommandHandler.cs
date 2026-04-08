using Application.Common;
using Application.DTOs.Auth;
using Domain.Constants;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Features.Auth.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtSettings _jwt;
        private const string Module = "Auth";

        public RegisterCommandHandler(
            UserManager<ApplicationUser> userManager,
            IOptions<JwtSettings> jwt)
        {
            _userManager = userManager;
            _jwt = jwt.Value;
        }

        public async Task<AuthResponseDto> Handle(
            RegisterCommand request,
            CancellationToken cancellationToken)
        {
            // ── 1. التحقق من صحة الـ Role ──────────────────────────────
            if (!AppRoles.AllowedForRegistration.Contains(request.Role))
                throw new BusinessLogicException(
                    $"Role '{request.Role}' is not allowed during self-registration. " +
                    $"Allowed roles: {string.Join(", ", AppRoles.AllowedForRegistration)}",
                    Module,
                    AppErrorCode.ValidationError);

            // ── 2. ImportOffice يحتاج CompanyName ───────────────────────
            if (request.Role == AppRoles.ImportOffice &&
                string.IsNullOrWhiteSpace(request.CompanyName))
                throw new BusinessLogicException(
                    "CompanyName is required for ImportOffice accounts.",
                    Module,
                    AppErrorCode.ValidationError);

            // ── 3. التحقق من تكرار الإيميل ──────────────────────────────
            var existing = await _userManager.FindByEmailAsync(request.Email);
            if (existing is not null)
                throw new BusinessLogicException(
                    "Email is already registered.",
                    Module,
                    AppErrorCode.EmailAlreadyExists);

            // ── 4. إنشاء المستخدم ───────────────────────────────────────
            // ImportOffice يحتاج موافقة Admin قبل التفعيل
            bool isActiveByDefault = request.Role != AppRoles.ImportOffice;

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                CompanyName = request.CompanyName,
                Address = request.Address,
                Country = request.Country,
                EmailConfirmed = true,
                IsActive = isActiveByDefault,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors
                    .ToDictionary(e => e.Code, e => new[] { e.Description });
                throw new ValidationException("Registration failed.", errors, Module);
            }

            // ── 5. تعيين الـ Role ────────────────────────────────────────
            var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                var errors = roleResult.Errors
                    .ToDictionary(e => e.Code, e => new[] { e.Description });
                throw new ValidationException("Failed to assign role.", errors, Module);
            }

            // ── 6. ImportOffice → رسالة انتظار موافقة Admin ─────────────
            if (!isActiveByDefault)
                return new AuthResponseDto
                {
                    Token = string.Empty,
                    FullName = user.FullName,
                    Email = user.Email!,
                    Role = request.Role,
                    ExpiresAt = DateTime.UtcNow,
                    Message = "Your account is pending Admin approval. You will be notified once activated."
                };

            return await BuildResponseAsync(user);
        }

        // ─── Helpers ────────────────────────────────────────────────────

        private async Task<AuthResponseDto> BuildResponseAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? AppRoles.Customer;
            var expires = DateTime.UtcNow.AddDays(_jwt.ExpiryDays);

            return new AuthResponseDto
            {
                Token = GenerateJwt(user, role, expires),
                FullName = user.FullName,
                Email = user.Email!,
                Role = role,
                ExpiresAt = expires
            };
        }

        private string GenerateJwt(ApplicationUser user, string role, DateTime expires)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name,               user.FullName),
                new Claim(ClaimTypes.Role,               role),
            };

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}