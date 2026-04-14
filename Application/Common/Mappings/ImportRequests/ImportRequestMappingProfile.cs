using Application.DTOs.ImportRequests;
using AutoMapper;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Mappings.ImportRequests
{
    public class ImportRequestMappingProfile : Profile
    {
        public ImportRequestMappingProfile()
        {
            CreateMap<ImportRequest, ImportRequestDto>()
                .ForMember(d => d.UserFullName,
                    o => o.MapFrom(s => s.User != null ? s.User.FullName : string.Empty))
                .ForMember(d => d.ProductName,
                    o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty))
                .ForMember(d => d.AssignedOfficeName,
                    o => o.MapFrom(s => s.AssignedOffice != null ? s.AssignedOffice.FullName : null))
                .ForMember(d => d.Status,
                    o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.ShipmentType,
                    o => o.MapFrom(s => s.ShipmentType.ToString()));

            CreateMap<ImportRequest, ImportRequestListDto>()
                .ForMember(d => d.ProductName,
                    o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty))
                .ForMember(d => d.Status,
                    o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.ShipmentType,
                    o => o.MapFrom(s => s.ShipmentType.ToString()))
                .ForMember(d => d.AssignedOfficeName,
                    o => o.MapFrom(s => s.AssignedOffice != null ? s.AssignedOffice.FullName : null));
        }
    }
}
