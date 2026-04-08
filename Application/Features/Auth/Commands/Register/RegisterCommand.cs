using Application.DTOs.Auth;
using Domain.Constants;
using MediatR;

namespace Application.Features.Auth.Commands.Register
{
    public record RegisterCommand(
        string FullName,
        string Email,
        string Password,
        string ConfirmPassword,
        string? PhoneNumber,
        string Role,           // Customer | ImportOffice | Exporter
        string? CompanyName,    // مطلوب لـ ImportOffice
        string? Address,
        string? Country
    ) : IRequest<AuthResponseDto>;
}