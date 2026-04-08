using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth
{
    public class RegisterRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        /// <summary>
        /// المسموح بيه: Customer | ImportOffice | Exporter
        /// لو مش موجود → Customer افتراضياً
        /// </summary>
        public string Role { get; set; } = AppRoles.Customer;

        /// <summary>مطلوب لو Role = ImportOffice</summary>
        public string? CompanyName { get; set; }

        /// <summary>مطلوب لو Role = ImportOffice</summary>
        public string? Address { get; set; }

        /// <summary>مطلوب لو Role = ImportOffice</summary>
        public string? Country { get; set; }
    }
}