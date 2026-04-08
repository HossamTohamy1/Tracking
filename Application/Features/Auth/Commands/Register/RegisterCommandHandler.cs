using Application.Common;
using Application.Common.Mappings;
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

namespace Application.Features.Auth.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtSettings _jwt;
        private readonly IMapper _mapper;
        private static readonly ILogger _logger = Log.ForContext<RegisterCommandHandler>();
        private const string Module = "Auth";

        public RegisterCommandHandler(
            UserManager<ApplicationUser> userManager,
            IOptions<JwtSettings> jwt,
            IMapper mapper)
        {
            _userManager = userManager;
            _jwt = jwt.Value;
            _mapper = mapper;
        }

        public async Task<AuthResponseDto> Handle(
            RegisterCommand request,
            CancellationToken cancellationToken)
        {
            _logger.Information("Register attempt for {Email} with role {Role}",
                request.Email, request.Role);

            if (!AppRoles.AllowedForRegistration.Contains(request.Role))
            {
                _logger.Warning("Invalid role attempted during registration: {Role}", request.Role);
                throw new BusinessLogicException(
                    $"Role '{request.Role}' is not allowed during self-registration. " +
                    $"Allowed roles: {string.Join(", ", AppRoles.AllowedForRegistration)}",
                    Module,
                    AppErrorCode.ValidationError);
            }

            if (request.Role == AppRoles.ImportOffice &&
                string.IsNullOrWhiteSpace(request.CompanyName))
            {
                _logger.Warning("CompanyName missing for ImportOffice registration: {Email}", request.Email);
                throw new BusinessLogicException(
                    "CompanyName is required for ImportOffice accounts.",
                    Module,
                    AppErrorCode.ValidationError);
            }

            var existing = await _userManager.FindByEmailAsync(request.Email);
            if (existing is not null)
            {
                _logger.Warning("Duplicate registration attempt for {Email}", request.Email);
                throw new BusinessLogicException(
                    "Email is already registered.",
                    Module,
                    AppErrorCode.EmailAlreadyExists);
            }

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
                _logger.Warning("User creation failed for {Email}: {@Errors}", request.Email, errors);
                throw new ValidationException("Registration failed.", errors, Module);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                var errors = roleResult.Errors
                    .ToDictionary(e => e.Code, e => new[] { e.Description });
                _logger.Error("Role assignment failed for {Email}: {@Errors}", request.Email, errors);
                throw new ValidationException("Failed to assign role.", errors, Module);
            }

            if (!isActiveByDefault)
            {
                _logger.Information("ImportOffice account pending approval: {Email}", request.Email);

                var pendingDto = _mapper.Map<AuthResponseDto>(user);
                pendingDto.Token = string.Empty;
                pendingDto.Role = request.Role;
                pendingDto.ExpiresAt = DateTime.UtcNow;
                pendingDto.Message = "Your account is pending Admin approval. You will be notified once activated.";
                return pendingDto;
            }

            _logger.Information("User registered successfully: {Email} as {Role}",
                user.Email, request.Role);

            return await BuildResponseAsync(user);
        }


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