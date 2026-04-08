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

namespace Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtSettings _jwt;
        private const string Module = "Auth";

        public LoginCommandHandler(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IOptions<JwtSettings> jwt)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwt = jwt.Value;
        }

        public async Task<AuthResponseDto> Handle(
            LoginCommand request,
            CancellationToken cancellationToken)
        {
            // ── 1. هل الإيميل موجود؟ ────────────────────────────────────
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
                throw new BusinessLogicException(
                    "Invalid email or password.",
                    Module,
                    AppErrorCode.InvalidCredentials);

            // ── 2. هل الحساب محذوف؟ ─────────────────────────────────────
            if (user.IsDeleted)
                throw new BusinessLogicException(
                    "Account not found.",
                    Module,
                    AppErrorCode.InvalidCredentials);

            // ── 3. هل الحساب مفعَّل؟ (ImportOffice بيحتاج موافقة Admin) ─
            if (!user.IsActive)
                throw new BusinessLogicException(
                    "Your account is not yet active. Please wait for Admin approval.",
                    Module,
                    AppErrorCode.AccountInactive);

            // ── 4. هل الحساب مقفول؟ (بعد محاولات خاطئة) ────────────────
            if (await _userManager.IsLockedOutAsync(user))
                throw new BusinessLogicException(
                    "Account is temporarily locked due to multiple failed attempts. Try again later.",
                    Module,
                    AppErrorCode.AccountLocked);

            // ── 5. التحقق من كلمة السر ──────────────────────────────────
            var result = await _signInManager.CheckPasswordSignInAsync(
                user, request.Password, lockoutOnFailure: true);

            if (!result.Succeeded)
                throw new BusinessLogicException(
                    "Invalid email or password.",
                    Module,
                    AppErrorCode.InvalidCredentials);

            // ── 6. تحديث LastLoginAt ─────────────────────────────────────
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

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