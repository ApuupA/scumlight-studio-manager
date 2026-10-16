using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.DataLayer.Models.GearModels;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.DataLayer.Models.SessionModels;
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
                .ForMember(dest => dest.GearCategoryName, opt => opt.MapFrom(src => src.GearCategory != null ? src.GearCategory.Name : string.Empty))
                .ForMember(dest => dest.InSessionTodayQuantity, opt => opt.Ignore()); // set by GearItemService

            CreateMap<GearItemCreateDto, GearItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PhotoPath, opt => opt.Ignore())
                .ForMember(dest => dest.GearCategory, opt => opt.Ignore())
                .ForMember(dest => dest.DateCreated, opt => opt.Ignore())
                .ForMember(dest => dest.DateUpdated, opt => opt.Ignore());

            // Users (the password hash never leaves the service layer)
            CreateMap<User, UserReadDto>();

            CreateClientMappings();
            CreateProjectMappings();
            CreateSessionMappings();
            CreateInstrumentMappings();
        }

        private void CreateInstrumentMappings()
        {
            CreateMap<ProjectInstrument, InstrumentShortDto>();

            CreateMap<ProjectInstrument, ProjectInstrumentReadDto>()
                .ForMember(dest => dest.BandName, opt => opt.MapFrom(src => src.Band != null ? src.Band.Name : null))
                .ForMember(dest => dest.SetupItems, opt => opt.MapFrom(src => src.SetupItems.OrderBy(s => s.SortOrder)));

            CreateMap<ProjectInstrumentCreateDto, ProjectInstrument>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Project, opt => opt.Ignore())
                .ForMember(dest => dest.Band, opt => opt.Ignore())
                .ForMember(dest => dest.SetupItems, opt => opt.Ignore())
                .ForMember(dest => dest.Sessions, opt => opt.Ignore())
                .ForMember(dest => dest.DateCreated, opt => opt.Ignore())
                .ForMember(dest => dest.DateUpdated, opt => opt.Ignore());

            // Source and DisplayName depend on which of the three possible sources is set
            CreateMap<InstrumentSetupItem, InstrumentSetupItemReadDto>()
                .ForMember(dest => dest.Source, opt => opt.MapFrom(src =>
                    src.GearItemId != null ? SetupItemSource.Studio
                    : src.ExternalGearId != null ? SetupItemSource.Band
                    : SetupItemSource.Custom))
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src =>
                    src.GearItem != null ? src.GearItem.Name
                    : src.ExternalGear != null ? src.ExternalGear.Name
                    : src.CustomName ?? string.Empty))
                .ForMember(dest => dest.GearCategoryName, opt => opt.MapFrom(src =>
                    src.GearItem != null && src.GearItem.GearCategory != null ? src.GearItem.GearCategory.Name : null))
                .ForMember(dest => dest.GearStatus, opt => opt.MapFrom(src => src.GearItem != null ? src.GearItem.Status : (GearStatus?)null))
                .ForMember(dest => dest.ExternalGearOwner, opt => opt.MapFrom(src => src.ExternalGear != null ? src.ExternalGear.Owner : null));

            CreateMap<InstrumentSetupItemCreateDto, InstrumentSetupItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.GearItem, opt => opt.Ignore())
                .ForMember(dest => dest.ExternalGear, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectInstrument, opt => opt.Ignore())
                .ForMember(dest => dest.DateCreated, opt => opt.Ignore())
                .ForMember(dest => dest.DateUpdated, opt => opt.Ignore());

            CreateMap<InstrumentSetupItem, GearUsageDto>()
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectInstrument!.ProjectId))
                .ForMember(dest => dest.ProjectTitle, opt => opt.MapFrom(src => src.ProjectInstrument!.Project!.Title))
                .ForMember(dest => dest.ProjectStatus, opt => opt.MapFrom(src => src.ProjectInstrument!.Project!.Status))
                .ForMember(dest => dest.InstrumentName, opt => opt.MapFrom(src => src.ProjectInstrument!.Name));

            CreateMap<SetupTemplate, SetupTemplateReadDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items.OrderBy(i => i.SortOrder)));

            CreateMap<SetupTemplateItem, SetupTemplateItemReadDto>()
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src =>
                    src.GearItem != null ? src.GearItem.Name : src.CustomName ?? string.Empty));
        }

        private void CreateClientMappings()
        {
            CreateMap<Client, ClientReadDto>()
                .ForMember(dest => dest.BandCount, opt => opt.MapFrom(src => src.Bands.Count))
                .ForMember(dest => dest.ProjectCount, opt => opt.MapFrom(src => src.Projects.Count));

            CreateMap<ClientCreateDto, Client>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Bands, opt => opt.Ignore())
                .ForMember(dest => dest.Projects, opt => opt.Ignore())
                .ForMember(dest => dest.DateCreated, opt => opt.Ignore())
                .ForMember(dest => dest.DateUpdated, opt => opt.Ignore());

            CreateMap<Band, BandReadDto>()
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Client != null ? src.Client.Name : string.Empty));
            CreateMap<Band, BandShortDto>();

            CreateMap<BandCreateDto, Band>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Client, opt => opt.Ignore())
                .ForMember(dest => dest.Projects, opt => opt.Ignore())
                .ForMember(dest => dest.DateCreated, opt => opt.Ignore())
                .ForMember(dest => dest.DateUpdated, opt => opt.Ignore());
        }

        private void CreateProjectMappings()
        {
            CreateMap<Project, ProjectReadDto>()
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Client != null ? src.Client.Name : string.Empty))
                .ForMember(dest => dest.SongCount, opt => opt.MapFrom(src => src.Songs.Count))
                .ForMember(dest => dest.SongsDoneCount, opt => opt.MapFrom(src => src.Songs.Count(s => s.Status == SongStatus.Done)))
                .ForMember(dest => dest.SessionCount, opt => opt.MapFrom(src => src.Sessions.Count))
                .ForMember(dest => dest.NextSessionDate, opt => opt.MapFrom(src => src.Sessions
                    .Where(s => s.Status != SessionStatus.Cancelled && s.LastDate >= DateOnly.FromDateTime(DateTime.Now))
                    .Select(s => (DateOnly?)s.Date)
                    .Min()));

            // The bands are set by ProjectService (they have to be loaded from the database)
            CreateMap<ProjectCreateDto, Project>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Client, opt => opt.Ignore())
                .ForMember(dest => dest.Bands, opt => opt.Ignore())
                .ForMember(dest => dest.Songs, opt => opt.Ignore())
                .ForMember(dest => dest.Sessions, opt => opt.Ignore())
                .ForMember(dest => dest.Links, opt => opt.Ignore())
                .ForMember(dest => dest.DrumTunings, opt => opt.Ignore())
                .ForMember(dest => dest.ExternalGear, opt => opt.Ignore())
                .ForMember(dest => dest.Instruments, opt => opt.Ignore())
                .ForMember(dest => dest.DateCreated, opt => opt.Ignore())
                .ForMember(dest => dest.DateUpdated, opt => opt.Ignore());

            CreateMap<Song, SongReadDto>()
                .ForMember(dest => dest.BandName, opt => opt.MapFrom(src => src.Band != null ? src.Band.Name : null));
            CreateMap<SongCreateDto, Song>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Project, opt => opt.Ignore())
                .ForMember(dest => dest.Band, opt => opt.Ignore())
                .ForMember(dest => dest.Sessions, opt => opt.Ignore())
                .ForMember(dest => dest.DateCreated, opt => opt.Ignore())
                .ForMember(dest => dest.DateUpdated, opt => opt.Ignore());

            CreateMap<ProjectLink, ProjectLinkReadDto>()
                .ForMember(dest => dest.SongTitle, opt => opt.MapFrom(src => src.Song != null ? src.Song.Title : null));
            CreateMap<ProjectLinkCreateDto, ProjectLink>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Project, opt => opt.Ignore())
                .ForMember(dest => dest.Song, opt => opt.Ignore())
                .ForMember(dest => dest.DateCreated, opt => opt.Ignore())
                .ForMember(dest => dest.DateUpdated, opt => opt.Ignore());

            CreateMap<DrumTuning, DrumTuningReadDto>()
                .ForMember(dest => dest.SongTitle, opt => opt.MapFrom(src => src.Song != null ? src.Song.Title : null));
            CreateMap<DrumTuningCreateDto, DrumTuning>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Project, opt => opt.Ignore())
                .ForMember(dest => dest.Song, opt => opt.Ignore())
                .ForMember(dest => dest.DateCreated, opt => opt.Ignore())
                .ForMember(dest => dest.DateUpdated, opt => opt.Ignore());

            CreateMap<ExternalGear, ExternalGearReadDto>();
            CreateMap<ExternalGearCreateDto, ExternalGear>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Project, opt => opt.Ignore())
                .ForMember(dest => dest.DateCreated, opt => opt.Ignore())
                .ForMember(dest => dest.DateUpdated, opt => opt.Ignore());
        }

        private void CreateSessionMappings()
        {
            CreateMap<Session, SessionReadDto>()
                .ForMember(dest => dest.ProjectTitle, opt => opt.MapFrom(src => src.Project != null ? src.Project.Title : string.Empty))
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src =>
                    src.Project != null && src.Project.Client != null ? src.Project.Client.Name : string.Empty))
                .ForMember(dest => dest.SongIds, opt => opt.MapFrom(src => src.Songs.Select(s => s.Id)))
                .ForMember(dest => dest.TaskCount, opt => opt.MapFrom(src => src.Tasks.Count))
                .ForMember(dest => dest.TasksDoneCount, opt => opt.MapFrom(src => src.Tasks.Count(t => t.IsDone)))
                .ForMember(dest => dest.Instruments, opt => opt.MapFrom(src => src.Instruments.OrderBy(i => i.SortOrder)));

            // The songs are set by SessionService (they have to be loaded from the database)
            CreateMap<SessionCreateDto, Session>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Project, opt => opt.Ignore())
                .ForMember(dest => dest.Songs, opt => opt.Ignore())
                .ForMember(dest => dest.Tasks, opt => opt.Ignore())
                .ForMember(dest => dest.Instruments, opt => opt.Ignore())
                .ForMember(dest => dest.DateCreated, opt => opt.Ignore())
                .ForMember(dest => dest.DateUpdated, opt => opt.Ignore());

            CreateMap<SessionTask, SessionTaskReadDto>();
            CreateMap<SessionTaskCreateDto, SessionTask>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Session, opt => opt.Ignore())
                .ForMember(dest => dest.DateCreated, opt => opt.Ignore())
                .ForMember(dest => dest.DateUpdated, opt => opt.Ignore());
        }
    }
}
