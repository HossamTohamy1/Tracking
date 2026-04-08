using Application.DTOs.Auth;
using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.Register;
using Application.ViewModel;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APi_Presentation.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // ─────────────────────────────────────────────────────────────────
        // POST api/auth/register
        // Customer / ImportOffice / Exporter
        // ─────────────────────────────────────────────────────────────────
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ResponseViewModel<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            var command = new RegisterCommand(
                FullName: dto.FullName,
                Email: dto.Email,
                Password: dto.Password,
                ConfirmPassword: dto.ConfirmPassword,
                PhoneNumber: dto.PhoneNumber,
                Role: dto.Role,
                CompanyName: dto.CompanyName,
                Address: dto.Address,
                Country: dto.Country
            );

            var result = await _mediator.Send(command);
            return Ok(ResponseViewModel<AuthResponseDto>.Success(result, "Registration successful."));
        }

        // ─────────────────────────────────────────────────────────────────
        // POST api/auth/login
        // All roles
        // ─────────────────────────────────────────────────────────────────
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ResponseViewModel<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var command = new LoginCommand(dto.Email, dto.Password);
            var result = await _mediator.Send(command);
            return Ok(ResponseViewModel<AuthResponseDto>.Success(result, "Login successful."));
        }

        // ─────────────────────────────────────────────────────────────────
        // GET api/auth/me  — بيرجع بيانات المستخدم الحالي
        // ─────────────────────────────────────────────────────────────────
        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var name = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            return Ok(ResponseViewModel<object>.Success(new
            {
                UserId = userId,
                Email = email,
                Name = name,
                Role = role
            }));
        }

        // ─────────────────────────────────────────────────────────────────
        // مثال: endpoint مخصوص للـ Admin فقط
        // GET api/auth/admin-only
        // ─────────────────────────────────────────────────────────────────
        [HttpGet("admin-only")]
        [Authorize(Roles = AppRoles.Admin)]
        public IActionResult AdminOnly()
            => Ok(ResponseViewModel<string>.Success("Welcome, Admin!"));

        // ─────────────────────────────────────────────────────────────────
        // مثال: endpoint للـ Admin و Support
        // GET api/auth/dashboard
        // ─────────────────────────────────────────────────────────────────
        [HttpGet("dashboard")]
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Support}")]
        public IActionResult Dashboard()
            => Ok(ResponseViewModel<string>.Success("Welcome to the dashboard."));
    }
}