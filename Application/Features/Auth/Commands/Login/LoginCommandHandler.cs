using Application.Common;
using Application.DTOs.Auth;
using AutoMapper;
using Domain.Constants;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Serilog;
using System.IdentityModel.Tokens.Jwt;

namespace Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtSettings _jwt;
        private readonly IMapper _mapper;
        private static readonly ILogger _logger = Log.ForContext<LoginCommandHandler>();
        private const string Module = "Auth";

        public LoginCommandHandler(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IOptions<JwtSettings> jwt,
            IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwt = jwt.Value;
            _mapper = mapper;
        }

        public async Task<AuthResponseDto> Handle(
            LoginCommand request,
            CancellationToken cancellationToken)
        {
            _logger.Information("Login attempt for {Email}", request.Email);

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                _logger.Warning("Login failed - email not found: {Email}", request.Email);
                throw new BusinessLogicException(
                    "Invalid email or password.", Module, AppErrorCode.InvalidCredentials);
            }

            if (user.IsDeleted)
            {
                _logger.Warning("Login attempt on deleted account: {Email}", request.Email);
                throw new BusinessLogicException(
                    "Account not found.", Module, AppErrorCode.InvalidCredentials);
            }

            if (!user.IsActive)
            {
                _logger.Warning("Login attempt on inactive account: {Email}", request.Email);
                throw new BusinessLogicException(
                    "Your account is not yet active. Please wait for Admin approval.",
                    Module, AppErrorCode.AccountInactive);
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.Warning("Login attempt on locked account: {Email}", request.Email);
                throw new BusinessLogicException(
                    "Account is temporarily locked due to multiple failed attempts. Try again later.",
                    Module, AppErrorCode.AccountLocked);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(
                user, request.Password, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                _logger.Warning("Login failed - wrong password for {Email}", request.Email);
                throw new BusinessLogicException(
                    "Invalid email or password.", Module, AppErrorCode.InvalidCredentials);
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            _logger.Information("User logged in successfully: {Email}", user.Email);

            return await BuildResponseAsync(user);
        }

        // ─── Helpers ────────────────────────────────────────────────────

        private async Task<AuthResponseDto> BuildResponseAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? AppRoles.Customer;
            var expires = DateTime.UtcNow.AddDays(_jwt.ExpiryDays);

            var dto = _mapper.Map<AuthResponseDto>(user);
            dto.Token = JwtHelper.Generate(user, role, expires, _jwt);
            dto.Role = role;
            dto.ExpiresAt = expires;
            return dto;
        }
    }
}