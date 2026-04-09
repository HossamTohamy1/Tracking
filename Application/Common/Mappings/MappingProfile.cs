// Application/Common/Mappings/MappingProfile.cs
using Application.DTOs.Auth;
using Application.DTOs.Products;
using Application.Features.Products.Commands.CreateProduct;
using Application.Features.Products.Commands.UpdateProduct;
using AutoMapper;
using Domain.Models;

namespace Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ApplicationUser → AuthResponseDto
            // Token, Role, ExpiresAt بيتملوا manually بعد الـ mapping
            CreateMap<ApplicationUser, AuthResponseDto>()
                .ForMember(dest => dest.Token, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.ExpiresAt, opt => opt.Ignore())
                .ForMember(dest => dest.Message, opt => opt.Ignore());

            CreateMap<Product, ProductDto>();

            CreateMap<CreateProductCommand, Product>();

            CreateMap<UpdateProductCommand, Product>()
                .ForAllMembers(opts =>
                    opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}