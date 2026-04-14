using Application.DTOs.PaymentDtos;
using AutoMapper;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Mappings
{
    public class PaymentMappingProfile : Profile
    {
        public PaymentMappingProfile()
        {
            CreateMap<Payment, PaymentResponseDto>()
                .ForMember(dest => dest.Purpose,
                    opt => opt.MapFrom(src => src.Purpose.ToString()))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Method,
                    opt => opt.MapFrom(src => src.Method.ToString()));
        }
    }
}
