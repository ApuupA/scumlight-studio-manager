using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.DataLayer.Models.FinanceModels;
using StudioManager.DataLayer.Models.GearModels;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.DataLayer.Models.SessionModels;
using StudioManager.DataLayer.Models.SettingsModels;
using StudioManager.DataLayer.Models.UserModels;
using StudioManager.ServiceLayer.Service.FinanceServices;

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
            CreateInputListMappings();
            CreateFinanceMappings();
        }

        private void CreateInputListMappings()
        {
            CreateMap<InputGroup, InputGroupReadDto>()
                .ForMember(dest => dest.InputCount, opt => opt.MapFrom(src => src.Inputs.Count));
            CreateMap<InputGroupCreateDto, InputGroup>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Inputs, opt => opt.Ignore())
                .ForMember(dest => dest.DateCreated, opt => opt.Ignore())
                .ForMember(dest => dest.DateUpdated, opt => opt.Ignore());

            CreateMap<StudioInput, StudioInputReadDto>()
                .ForMember(dest => dest.InputGroupName, opt => opt.MapFrom(src => src.InputGroup != null ? src.InputGroup.Name : null))
                .ForMember(dest => dest.InputGroupColor, opt => opt.MapFrom(src => src.InputGroup != null ? src.InputGroup.Color : null));
            CreateMap<StudioInputCreateDto, StudioInput>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.InputGroup, opt => opt.Ignore())
                .ForMember(dest => dest.DateCreated, opt => opt.Ignore())
                .ForMember(dest => dest.DateUpdated, opt => opt.Ignore());

            // The rows (one per studio input) are built by InputListService
            CreateMap<InputList, InputListReadDto>()
                .ForMember(dest => dest.Rows, opt => opt.Ignore())
                .ForMember(dest => dest.UsedInputCount, opt => opt.Ignore());
            CreateMap<InputListCreateDto, InputList>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Project, opt => opt.Ignore())
                .ForMember(dest => dest.Rows, opt => opt.Ignore())
                .ForMember(dest => dest.Sessions, opt => opt.Ignore())
                .ForMember(dest => dest.DateCreated, opt => opt.Ignore())
                .ForMember(dest => dest.DateUpdated, opt => opt.Ignore());

            CreateMap<InputListRowSaveDto, InputListRow>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.InputListId, opt => opt.Ignore())
                .ForMember(dest => dest.InputList, opt => opt.Ignore())
                .ForMember(dest => dest.StudioInputId, opt => opt.Ignore())
                .ForMember(dest => dest.StudioInput, opt => opt.Ignore())
                .ForMember(dest => dest.SetupItemId, opt => opt.Ignore())
                .ForMember(dest => dest.SetupItem, opt => opt.Ignore())
                .ForMember(dest => dest.DateCreated, opt => opt.Ignore())
                .ForMember(dest => dest.DateUpdated, opt => opt.Ignore());
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
                .ForMember(dest => dest.ProjectCount, opt => opt.MapFrom(src => src.Projects.Count))
                .ForMember(dest => dest.HasPhoto, opt => opt.MapFrom(src => src.PhotoPath != null))
                .ForMember(dest => dest.FallbackPhoto, opt => opt.MapFrom(src =>
                    src.PhotoPath == null && src.Bands.Count == 1 && src.Bands.First().PhotoPath != null
                        ? new PhotoRefDto { Id = src.Bands.First().Id, DateUpdated = src.Bands.First().DateUpdated }
                        : null));

            CreateMap<ClientCreateDto, Client>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PhotoPath, opt => opt.Ignore())
                .ForMember(dest => dest.Bands, opt => opt.Ignore())
                .ForMember(dest => dest.Projects, opt => opt.Ignore())
                .ForMember(dest => dest.DateCreated, opt => opt.Ignore())
                .ForMember(dest => dest.DateUpdated, opt => opt.Ignore());

            CreateMap<Band, BandReadDto>()
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Client != null ? src.Client.Name : string.Empty))
                .ForMember(dest => dest.HasPhoto, opt => opt.MapFrom(src => src.PhotoPath != null))
                .ForMember(dest => dest.FallbackPhoto, opt => opt.MapFrom(src =>
                    src.PhotoPath == null && src.Client != null && src.Client.PhotoPath != null
                        ? new PhotoRefDto { Id = src.Client.Id, DateUpdated = src.Client.DateUpdated }
                        : null));
            CreateMap<Band, BandShortDto>();

            CreateMap<BandCreateDto, Band>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PhotoPath, opt => opt.Ignore())
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
                .ForMember(dest => dest.HasPhoto, opt => opt.MapFrom(src => src.PhotoPath != null))
                .ForMember(dest => dest.NextSessionDate, opt => opt.MapFrom(src => src.Sessions
                    .Where(s => s.Status != SessionStatus.Cancelled && s.LastDate >= DateOnly.FromDateTime(DateTime.Now))
                    .Select(s => (DateOnly?)s.Date)
                    .Min()));

            // The bands are set by ProjectService (they have to be loaded from the database)
            // Pricing is set in the tab "Time & costs" (FinanceService.SetPricing)
            CreateMap<ProjectCreateDto, Project>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PricingMode, opt => opt.Ignore())
                .ForMember(dest => dest.HourlyRate, opt => opt.Ignore())
                .ForMember(dest => dest.FlatPrice, opt => opt.Ignore())
                .ForMember(dest => dest.PhotoPath, opt => opt.Ignore())
                .ForMember(dest => dest.BandGearToken, opt => opt.Ignore())
                .ForMember(dest => dest.Client, opt => opt.Ignore())
                .ForMember(dest => dest.Bands, opt => opt.Ignore())
                .ForMember(dest => dest.Songs, opt => opt.Ignore())
                .ForMember(dest => dest.Sessions, opt => opt.Ignore())
                .ForMember(dest => dest.Links, opt => opt.Ignore())
                .ForMember(dest => dest.DrumTunings, opt => opt.Ignore())
                .ForMember(dest => dest.ExternalGear, opt => opt.Ignore())
                .ForMember(dest => dest.Instruments, opt => opt.Ignore())
                .ForMember(dest => dest.InputLists, opt => opt.Ignore())
                .ForMember(dest => dest.DateCreated, opt => opt.Ignore())
                .ForMember(dest => dest.DateUpdated, opt => opt.Ignore());

            CreateMap<Song, SongReadDto>()
                .ForMember(dest => dest.BandName, opt => opt.MapFrom(src => src.Band != null ? src.Band.Name : null))
                .ForMember(dest => dest.Tempos, opt => opt.MapFrom(src => src.Tempos.OrderBy(t => t.SortOrder)))
                .ForMember(dest => dest.TrackCount, opt => opt.MapFrom(src => src.Tracks.Count))
                .ForMember(dest => dest.TracksRecordedCount, opt => opt.MapFrom(src => src.Tracks.Count(t => t.Status != SongTrackStatus.Open)));
            CreateMap<SongTempo, SongTempoReadDto>();
            // The tempos are replaced by SongService (ReplaceTempos)
            // Lyrics, arrangement and recording checklist are edited on the song page with their own endpoints
            CreateMap<SongCreateDto, Song>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Tempos, opt => opt.Ignore())
                .ForMember(dest => dest.Lyrics, opt => opt.Ignore())
                .ForMember(dest => dest.Sections, opt => opt.Ignore())
                .ForMember(dest => dest.Tracks, opt => opt.Ignore())
                .ForMember(dest => dest.Project, opt => opt.Ignore())
                .ForMember(dest => dest.Band, opt => opt.Ignore())
                .ForMember(dest => dest.Sessions, opt => opt.Ignore())
                .ForMember(dest => dest.DateCreated, opt => opt.Ignore())
                .ForMember(dest => dest.DateUpdated, opt => opt.Ignore());

            CreateMap<SongSection, SongSectionReadDto>();
            CreateMap<SongSectionCreateDto, SongSection>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Song, opt => opt.Ignore())
                .ForMember(dest => dest.DateCreated, opt => opt.Ignore())
                .ForMember(dest => dest.DateUpdated, opt => opt.Ignore());

            CreateMap<SongTrack, SongTrackReadDto>();
            CreateMap<SongTrackCreateDto, SongTrack>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Song, opt => opt.Ignore())
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
                // Only set by the band gear link / ExternalGearService.Accept - the form does not change it
                .ForMember(dest => dest.IsSuggestion, opt => opt.Ignore())
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
                .ForMember(dest => dest.Instruments, opt => opt.MapFrom(src => src.Instruments.OrderBy(i => i.SortOrder)))
                .ForMember(dest => dest.InputListName, opt => opt.MapFrom(src => src.InputList != null ? src.InputList.Name : null));

            // The songs are set by SessionService (they have to be loaded from the database)
            CreateMap<SessionCreateDto, Session>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Project, opt => opt.Ignore())
                .ForMember(dest => dest.InputList, opt => opt.Ignore())
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

        private void CreateFinanceMappings()
        {
            CreateMap<BusinessSettings, BusinessSettingsReadDto>()
                .ForMember(dest => dest.NextInvoiceNumberPreview, opt => opt.Ignore()) // set by FinanceService
                .ForMember(dest => dest.NextQuoteNumberPreview, opt => opt.Ignore());
            CreateMap<BusinessSettingsUpdateDto, BusinessSettings>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Currency, opt => opt.Ignore())
                .ForMember(dest => dest.DateCreated, opt => opt.Ignore())
                .ForMember(dest => dest.DateUpdated, opt => opt.Ignore());

            CreateMap<TimeEntry, TimeEntryReadDto>()
                .ForMember(dest => dest.IsRunning, opt => opt.MapFrom(src => src.StartedAt != null))
                .ForMember(dest => dest.ProjectTitle, opt => opt.MapFrom(src => src.Project != null ? src.Project.Title : string.Empty))
                .ForMember(dest => dest.SessionTitle, opt => opt.MapFrom(src => src.Session != null ? src.Session.Title : null))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.DisplayName : null))
                .ForMember(dest => dest.StandardTaskName, opt => opt.MapFrom(src => src.StandardTask != null ? src.StandardTask.Name : null))
                .ForMember(dest => dest.InvoiceNumber, opt => opt.MapFrom(src => src.Invoice != null ? src.Invoice.Number : null));
            CreateMap<TimeEntryCreateDto, TimeEntry>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore())
                .ForMember(dest => dest.StandardTask, opt => opt.Ignore())
                .ForMember(dest => dest.StartedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Project, opt => opt.Ignore())
                .ForMember(dest => dest.Session, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.DateCreated, opt => opt.Ignore())
                .ForMember(dest => dest.DateUpdated, opt => opt.Ignore());

            CreateMap<StandardTask, StandardTaskReadDto>();
            CreateMap<StandardTaskCreateDto, StandardTask>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.DateCreated, opt => opt.Ignore())
                .ForMember(dest => dest.DateUpdated, opt => opt.Ignore());

            CreateMap<ProjectCostItem, ProjectCostItemReadDto>()
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src => Math.Round(src.Quantity * src.UnitPrice, 2)))
                .ForMember(dest => dest.InvoiceNumber, opt => opt.MapFrom(src => src.Invoice != null ? src.Invoice.Number : null));
            CreateMap<Expense, ExpenseReadDto>()
                .ForMember(dest => dest.NetAmount, opt => opt.MapFrom(src => ExpenseService.NetOf(src.GrossAmount, src.VatRate)))
                .ForMember(dest => dest.VatAmount, opt => opt.MapFrom(src => src.GrossAmount - ExpenseService.NetOf(src.GrossAmount, src.VatRate)))
                .ForMember(dest => dest.IsAboveDepreciationLimit, opt => opt.MapFrom(src => ExpenseService.IsAboveDepreciationLimit(src)))
                .ForMember(dest => dest.ProjectTitle, opt => opt.MapFrom(src => src.Project != null ? src.Project.Title : null))
                .ForMember(dest => dest.HasReceipt, opt => opt.MapFrom(src => src.ReceiptPath != null));
            CreateMap<ExpenseCreateDto, Expense>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Project, opt => opt.Ignore())
                .ForMember(dest => dest.ReceiptPath, opt => opt.Ignore())
                .ForMember(dest => dest.ReceiptFileName, opt => opt.Ignore())
                .ForMember(dest => dest.DateCreated, opt => opt.Ignore())
                .ForMember(dest => dest.DateUpdated, opt => opt.Ignore());

            CreateMap<ProjectCostItemCreateDto, ProjectCostItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore())
                .ForMember(dest => dest.Project, opt => opt.Ignore())
                .ForMember(dest => dest.DateCreated, opt => opt.Ignore())
                .ForMember(dest => dest.DateUpdated, opt => opt.Ignore());
        }
    }
}
