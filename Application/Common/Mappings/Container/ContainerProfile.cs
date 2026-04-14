using Application.DTOs.ContainerDtos;
using AutoMapper;
using Domain.Models;
using System;

public class ContainerProfile : Profile
{
    public ContainerProfile()
    {
        // ═══════════════════════════════════════════════════════════════════════
        // ENTITY TO DTO MAPPINGS
        // ═══════════════════════════════════════════════════════════════════════

        // ── Container → ContainerDto (Full Detail) ────────────────────
        CreateMap<Container, ContainerDto>()
            .ForMember(dest => dest.StatusName,
                opt => opt.MapFrom(src => src.Status.ToString()))

            .ForMember(dest => dest.ManagedByOfficeName,
                opt => opt.MapFrom(src =>
                    src.ManagedByOffice != null
                        ? src.ManagedByOffice.FullName
                        : string.Empty))

            .ForMember(dest => dest.WeightUtilizationPercent,
                opt => opt.MapFrom(src =>
                    src.MaxWeightKg > 0
                        ? Math.Round((src.CurrentWeightKg / src.MaxWeightKg) * 100, 1)
                        : 0))

            .ForMember(dest => dest.VolumeUtilizationPercent,
                opt => opt.MapFrom(src =>
                    src.MaxVolumeCbm > 0
                        ? Math.Round((src.CurrentVolumeCbm / src.MaxVolumeCbm) * 100, 1)
                        : 0))

            .ForMember(dest => dest.Items,
                opt => opt.MapFrom(src =>
                    src.Items != null
                        ? src.Items.Where(i => !i.IsDeleted).ToList()
                        : new List<ContainerItem>()));

        // ── ContainerItem → ContainerItemDto ─────────────────────────
        CreateMap<ContainerItem, ContainerItemDto>()
       .ForMember(dest => dest.RequestNumber,
           opt => opt.MapFrom(src =>
               src.ImportRequest != null
                   ? src.ImportRequest.Id.ToString().Substring(0, 8)
                   : src.ImportRequestId.ToString().Substring(0, 8)));

        // ── Container → ContainerListItemDto (Paginated List) ─────────
        CreateMap<Container, ContainerListItemDto>()
            .ForMember(dest => dest.StatusName,
                opt => opt.MapFrom(src => src.Status.ToString()))

            // ✅ FIX: بيحسب بس الـ items اللي مش deleted
            .ForMember(dest => dest.ItemCount,
                opt => opt.MapFrom(src =>
                    src.Items != null
                        ? src.Items.Count(i => !i.IsDeleted)
                        : 0));

        // ═══════════════════════════════════════════════════════════════════════
        // REQUEST TO ENTITY MAPPINGS
        // ═══════════════════════════════════════════════════════════════════════

        CreateMap<CreateContainerRequest, Container>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(_ => false));

        CreateMap<UpdateContainerRequest, Container>()
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}