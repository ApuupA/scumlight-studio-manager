using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.GearModels;
using StudioManager.DataLayer.Models.UserModels;

namespace StudioManager.ServiceLayer.Mapper
{
    /// <summary>
    /// All entity to DTO mappings. MappingProfileTests checks that no property is forgotten.
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Gear categories
            CreateMap<GearCategory, GearCategoryReadDto>()
                .ForMember(dest => dest.GearItemCount, opt => opt.MapFrom(src => src.GearItems.Count));

            CreateMap<GearCategoryCreateDto, GearCategory>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.GearItems, opt => opt.Ignore())
                .ForMember(dest => dest.DateCreated, opt => opt.Ignore())
                .ForMember(dest => dest.DateUpdated, opt => opt.Ignore());

            // Gear items
            CreateMap<GearItem, GearItemReadDto>()
                .ForMember(dest => dest.HasPhoto, opt => opt.MapFrom(src => src.PhotoPath != null))
                .ForMember(dest => dest.GearCategoryName, opt => opt.MapFrom(src => src.GearCategory != null ? src.GearCategory.Name : string.Empty));

            CreateMap<GearItemCreateDto, GearItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PhotoPath, opt => opt.Ignore())
                .ForMember(dest => dest.GearCategory, opt => opt.Ignore())
                .ForMember(dest => dest.DateCreated, opt => opt.Ignore())
                .ForMember(dest => dest.DateUpdated, opt => opt.Ignore());

            // Users (the password hash never leaves the service layer)
            CreateMap<User, UserReadDto>();
        }
    }
}
