using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.IProjectServices;

namespace StudioManager.ServiceLayer.Service.ProjectServices
{
    public class ClientLinkService : IClientLinkService
    {
        /// <summary>Protects against a flood of entries through the public page.</summary>
        public const int MaxClientFeedbackPerProject = 2000;
        public const int MaxClientReferencesPerProject = 200;

        /// <summary>The comment written when the client approves a mix (shown in the feedback of the version).</summary>
        public const string ApprovedText = "Mix freigegeben";

        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<Song> _songRepository;
        private readonly IRepository<MixVersion> _versionRepository;
        private readonly IRepository<MixFeedback> _feedbackRepository;
        private readonly IRepository<ProjectLink> _linkRepository;
        private readonly IMixVersionService _mixVersionService;

        public ClientLinkService(
            IRepository<Project> projectRepository,
            IRepository<Song> songRepository,
            IRepository<MixVersion> versionRepository,
            IRepository<MixFeedback> feedbackRepository,
            IRepository<ProjectLink> linkRepository,
            IMixVersionService mixVersionService)
        {
            _projectRepository = projectRepository;
            _songRepository = songRepository;
            _versionRepository = versionRepository;
            _feedbackRepository = feedbackRepository;
            _linkRepository = linkRepository;
            _mixVersionService = mixVersionService;
        }

        // ---------- For the client (public page) ----------

        public async Task<ClientPageDto?> GetPage(string token)
        {
            var project = await FindByToken(token);
            return project == null ? null : await BuildPage(project);
        }

        public async Task<ClientPageDto> AddFeedback(string token, ClientFeedbackCreateDto dto)
        {
            var project = await FindByToken(token) ?? throw new EntityNotFoundException("ClientLink", 0);
            var version = await LoadVersion(project.Id, dto.MixVersionId);

            if (dto.SongId != null && !version.Songs.Any(s => s.Id == dto.SongId))
            {
                throw new BusinessRuleException("mixFeedback.songNotInVersion", "The song is not in this mix version.");
            }
            await EnsureFeedbackLimit(project.Id);

            await _feedbackRepository.AddAsync(new MixFeedback
            {
                MixVersionId = version.Id,
                SongId = dto.SongId,
                Timecode = string.IsNullOrWhiteSpace(dto.Timecode) ? null : dto.Timecode.Trim(),
                Text = dto.Text.Trim(),
                Author = dto.AuthorName.Trim(),
                IsFromClient = true,
                IsUnread = true
            });

            return await BuildPage(project);
        }

        public async Task<ClientPageDto> ApproveVersion(string token, int mixVersionId, ClientApproveDto dto)
        {
            var project = await FindByToken(token) ?? throw new EntityNotFoundException("ClientLink", 0);
            var version = await LoadVersion(project.Id, mixVersionId);
            await EnsureFeedbackLimit(project.Id);

            version.Status = MixVersionStatus.Approved;
            version.ApprovedBy = dto.AuthorName.Trim();
            await _versionRepository.UpdateAsync(version);

            // Also as a (ticked off) comment: the studio sees it under "new from the client", and it stays in the history
            await _feedbackRepository.AddAsync(new MixFeedback
            {
                MixVersionId = version.Id,
                Text = ApprovedText,
                Author = version.ApprovedBy,
                IsDone = true,
                IsFromClient = true,
                IsUnread = true
            });

            return await BuildPage(project);
        }

        public async Task<ClientPageDto> AddReference(string token, ClientReferenceCreateDto dto)
        {
            var project = await FindByToken(token) ?? throw new EntityNotFoundException("ClientLink", 0);

            if (dto.SongId != null && !await _songRepository.ExistsAsync(s => s.Id == dto.SongId && s.ProjectId == project.Id))
            {
                throw new EntityNotFoundException(nameof(Song), dto.SongId.Value);
            }

            var clientReferences = await _linkRepository.GetAllAsync(l => l.ProjectId == project.Id && l.AddedBy != null);
            if (clientReferences.Count() >= MaxClientReferencesPerProject)
            {
                throw new BusinessRuleException("clientLink.tooManyEntries", "There are too many entries - please contact the studio.");
            }

            await _linkRepository.AddAsync(new ProjectLink
            {
                ProjectId = project.Id,
                Type = ProjectLinkType.Reference,
                Title = dto.Title.Trim(),
                Url = dto.Url.Trim(),
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
                SongId = dto.SongId,
                AddedBy = dto.AuthorName.Trim(),
                IsUnread = true
            });

            return await BuildPage(project);
        }

        // ---------- For the studio ----------

        public async Task<IEnumerable<ClientNewsDto>> GetNews()
        {
            var feedback = await _feedbackRepository.GetAllWithPathsAsync(f => f.IsUnread, "MixVersion.Project");
            var links = await _linkRepository.GetAllAsync(l => l.IsUnread, l => l.Project!);

            var entries = feedback
                .Select(f => new { Project = f.MixVersion!.Project!, IsReference = false, f.Author, f.DateCreated })
                .Concat(links.Select(l => new { Project = l.Project!, IsReference = true, Author = l.AddedBy, l.DateCreated }));

            return entries
                .GroupBy(e => e.Project.Id)
                .Select(g => new ClientNewsDto
                {
                    ProjectId = g.Key,
                    ProjectTitle = g.First().Project.Title,
                    FeedbackCount = g.Count(e => !e.IsReference),
                    ReferenceCount = g.Count(e => e.IsReference),
                    Authors = g.Select(e => e.Author).Where(a => !string.IsNullOrEmpty(a)).Distinct().Order().ToList()!,
                    LatestAt = g.Max(e => e.DateCreated)
                })
                .OrderByDescending(n => n.LatestAt)
                .ToList();
        }

        public async Task MarkRead(int projectId)
        {
            var feedback = (await _feedbackRepository.GetAllWithPathsAsync(f => f.IsUnread && f.MixVersion!.ProjectId == projectId)).ToList();
            foreach (var item in feedback)
            {
                item.IsUnread = false;
            }
            await _feedbackRepository.UpdateRangeAsync(feedback);

            var links = (await _linkRepository.GetAllAsync(l => l.IsUnread && l.ProjectId == projectId)).ToList();
            foreach (var link in links)
            {
                link.IsUnread = false;
            }
            await _linkRepository.UpdateRangeAsync(links);
        }

        // ---------- Helpers ----------

        /// <summary>
        /// The page: the mix versions (built by MixVersionService, so "all songs" and the feedback are the same
        /// as in the studio) and the references. Only shown parts: no prices, no internal song notes.
        /// </summary>
        private async Task<ClientPageDto> BuildPage(Project project)
        {
            var songs = (await _songRepository.GetAllAsync(s => s.ProjectId == project.Id))
                .OrderBy(s => s.TrackNumber).ThenBy(s => s.Title)
                .Select(s => new SongShortDto { Id = s.Id, TrackNumber = s.TrackNumber, Title = s.Title })
                .ToList();
            var versions = project.IncludesMixing ? await _mixVersionService.GetForProject(project.Id) : [];
            var references = await _linkRepository.GetAllAsync(l => l.ProjectId == project.Id && l.Type == ProjectLinkType.Reference, l => l.Song!);

            return new ClientPageDto
            {
                ProjectTitle = project.Title,
                BandNames = string.Join(", ", project.Bands.OrderBy(b => b.Name).Select(b => b.Name)),
                ShowMixing = project.IncludesMixing,
                ShowGear = project.IncludesRecording,
                Songs = songs,
                MixVersions = versions.Select(v => new ClientMixVersionDto
                {
                    Id = v.Id,
                    Name = v.Name,
                    Date = v.Date,
                    Status = v.Status,
                    Link = v.Link,
                    Notes = v.Notes,
                    IsAllSongs = v.IsAllSongs,
                    Songs = v.Songs,
                    ApprovedBy = v.ApprovedBy,
                    Feedback = v.Feedback.Select(f => new ClientFeedbackDto
                    {
                        Author = f.Author,
                        IsFromClient = f.IsFromClient,
                        SongTitle = f.SongTitle,
                        Timecode = f.Timecode,
                        Text = f.Text,
                        IsDone = f.IsDone,
                        DateCreated = f.DateCreated
                    }).ToList()
                }).ToList(),
                References = references
                    .OrderBy(l => l.Title)
                    .Select(l => new ClientReferenceDto
                    {
                        Title = l.Title,
                        Url = l.Url,
                        Notes = l.Notes,
                        SongTitle = l.Song?.Title,
                        AddedBy = l.AddedBy
                    }).ToList()
            };
        }

        /// <summary>A version of this project (with its songs) - versions of other projects are "not found".</summary>
        private async Task<MixVersion> LoadVersion(int projectId, int mixVersionId)
        {
            return (await _versionRepository.GetAllWithPathsAsync(v => v.Id == mixVersionId && v.ProjectId == projectId, "Songs")).SingleOrDefault()
                ?? throw new EntityNotFoundException(nameof(MixVersion), mixVersionId);
        }

        private async Task EnsureFeedbackLimit(int projectId)
        {
            var count = (await _feedbackRepository.GetAllWithPathsAsync(f => f.IsFromClient && f.MixVersion!.ProjectId == projectId)).Count();
            if (count >= MaxClientFeedbackPerProject)
            {
                throw new BusinessRuleException("clientLink.tooManyEntries", "There are too many entries - please contact the studio.");
            }
        }

        private async Task<Project?> FindByToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }
            return await _projectRepository.GetAsync(p => p.BandGearToken == token, p => p.Bands);
        }
    }
}
