using Application.DTOs.CostCalculationDtos;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.CostCalculation
{
    public class CostCalculationMappingProfile : Profile
    {
        public CostCalculationMappingProfile()
        {
            // ── CostCalculation → CostCalculationDto ─────────────────────────
            // ProjectTo will translate these member mappings to SQL via EF
            CreateMap<Domain.Models.CostCalculation, CostCalculationDto>()
                // From ImportRequest navigation (projection-friendly flat access)
                .ForMember(dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.ImportRequest.User.FullName))
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.ImportRequest.Product.Name))
                // IsLocked = any Completed payment for this request
                .ForMember(dest => dest.IsLocked,
                    opt => opt.MapFrom(src =>
                        src.ImportRequest.Payments
                            .Any(p => p.Status == Domain.Enums.Enums_Models.PaymentStatus.Completed)));
        }
    }
}