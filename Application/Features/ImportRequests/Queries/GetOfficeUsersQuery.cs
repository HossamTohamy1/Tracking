using Application.ViewModel;
using Domain.Constants;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Features.Users.Queries
{
    public record GetOfficeUsersQuery : IRequest<ResponseViewModel<List<OfficeUserDto>>>;

    public class OfficeUserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
    }

    public class GetOfficeUsersQueryHandler
        : IRequestHandler<GetOfficeUsersQuery, ResponseViewModel<List<OfficeUserDto>>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<GetOfficeUsersQueryHandler> _logger;

        public GetOfficeUsersQueryHandler(
            UserManager<ApplicationUser> userManager,
            ILogger<GetOfficeUsersQueryHandler> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<OfficeUserDto>>> Handle(
            GetOfficeUsersQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Admin fetching all ImportOffice users.");

            var officeUsers = await _userManager.GetUsersInRoleAsync(AppRoles.ImportOffice);

            var result = officeUsers
                .Where(u => u.IsActive)
                .Select(u => new OfficeUserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email ?? string.Empty,
                    CompanyName = u.CompanyName,
                })
                .OrderBy(u => u.FullName)
                .ToList();

            return ResponseViewModel<List<OfficeUserDto>>.Success(result);
        }
    }
}