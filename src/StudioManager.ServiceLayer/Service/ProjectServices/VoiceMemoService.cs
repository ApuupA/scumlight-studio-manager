using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.IFileServices;
using StudioManager.ServiceLayer.IService.IProjectServices;
using StudioManager.ServiceLayer.Results;
using System.Linq.Expressions;

namespace StudioManager.ServiceLayer.Service.ProjectServices
{
    public class VoiceMemoService : BaseService<VoiceMemo, VoiceMemoReadDto, VoiceMemoCreateDto>, IVoiceMemoService
    {
        public const string MemoFolder = "voice-memos";

        /// <summary>
        /// Max. size of one memo. Matches client_max_body_size in web/nginx.conf - about 20 minutes of a phone recording.
        /// </summary>
        public const long MaxMemoSizeBytes = 20 * 1024 * 1024;

        /// <summary>
        /// Allowed file extensions (the usual formats of iPhone and Android recorders) and the content type for playing.
        /// </summary>
        private static readonly Dictionary<string, string> AudioContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            [".m4a"] = "audio/mp4",
            [".mp3"] = "audio/mpeg",
            [".wav"] = "audio/wav",
            [".aac"] = "audio/aac",
            [".ogg"] = "audio/ogg",
            [".opus"] = "audio/ogg",
            [".flac"] = "audio/flac",
            [".webm"] = "audio/webm"
        };

        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<Song> _songRepository;
        private readonly IRepository<SongSection> _sectionRepository;
        private readonly IFileStorageService _fileStorage;

        public VoiceMemoService(
            IRepository<VoiceMemo> repository,
            IRepository<Project> projectRepository,
            IRepository<Song> songRepository,
            IRepository<SongSection> sectionRepository,
            IFileStorageService fileStorage,
            IMapper mapper)
            : base(repository, mapper)
        {
            _projectRepository = projectRepository;
            _songRepository = songRepository;
            _sectionRepository = sectionRepository;
            _fileStorage = fileStorage;
        }

        // Load song and part, so their names can be shown
        protected override Expression<Func<VoiceMemo, object>>[] DefaultIncludes => [m => m.Song!, m => m.SongSection!];

        public async Task<IEnumerable<VoiceMemoReadDto>> GetForProject(int projectId)
        {
            var memos = await GetAllWhere(m => m.ProjectId == projectId);
            return memos
                .OrderByDescending(m => m.RecordedOn ?? DateOnly.FromDateTime(m.DateCreated))
                .ThenByDescending(m => m.Id);
        }

        public async Task<VoiceMemoReadDto> Upload(int projectId, Stream content, string fileName, long length, DateOnly? recordedOn, int? songId)
        {
            if (!await _projectRepository.ExistsAsync(p => p.Id == projectId))
            {
                throw new EntityNotFoundException(nameof(Project), projectId);
            }

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!AudioContentTypes.ContainsKey(extension))
            {
                throw new BusinessRuleException("voiceMemo.invalidType", "Only M4A, MP3, WAV, AAC, OGG, OPUS, FLAC and WEBM files are allowed.");
            }

            if (length > MaxMemoSizeBytes)
            {
                throw new BusinessRuleException("voiceMemo.tooLarge", "A voice memo must not be larger than 20 MB.");
            }

            if (songId != null)
            {
                await CheckSongAndSection(projectId, songId, null);
            }

            var memo = new VoiceMemo
            {
                ProjectId = projectId,
                SongId = songId,
                Title = TitleOf(fileName),
                RecordedOn = recordedOn,
                FileName = Path.GetFileName(fileName),
                FileSize = length,
                FilePath = await _fileStorage.SaveAsync(content, MemoFolder, extension)
            };
            await _repository.AddAsync(memo);

            return (await Get(memo.Id))!;
        }

        public async Task<FileDownload?> GetFile(int id)
        {
            var memo = await _repository.GetAsync(id) ?? throw new EntityNotFoundException(nameof(VoiceMemo), id);

            var stream = _fileStorage.OpenRead(memo.FilePath);
            if (stream == null)
            {
                return null;
            }

            var contentType = AudioContentTypes.GetValueOrDefault(Path.GetExtension(memo.FilePath), "application/octet-stream");
            return new FileDownload(stream, contentType, memo.FileName);
        }

        /// <summary>
        /// A memo is only created by the upload - it needs its audio file.
        /// </summary>
        public override Task<VoiceMemoReadDto> Insert(VoiceMemoCreateDto createDto)
        {
            throw new BusinessRuleException("voiceMemo.fileRequired", "A voice memo is created by uploading its audio file.");
        }

        public override async Task<VoiceMemoReadDto> Update(int id, VoiceMemoCreateDto updateDto)
        {
            var memo = await _repository.GetAsync(id) ?? throw new EntityNotFoundException(nameof(VoiceMemo), id);

            // A memo stays in its project
            updateDto.ProjectId = memo.ProjectId;
            updateDto.Title = updateDto.Title.Trim();
            updateDto.Notes = string.IsNullOrWhiteSpace(updateDto.Notes) ? null : updateDto.Notes.Trim();
            updateDto.SongId = await CheckSongAndSection(memo.ProjectId, updateDto.SongId, updateDto.SongSectionId);
            if (updateDto.SongId == null)
            {
                updateDto.SongSectionId = null;
            }

            return await base.Update(id, updateDto);
        }

        public override async Task Delete(int id)
        {
            var memo = await _repository.GetAsync(id) ?? throw new EntityNotFoundException(nameof(VoiceMemo), id);
            await base.Delete(id);

            // The audio file belongs to the memo - delete it after the memo is gone
            _fileStorage.Delete(memo.FilePath);
        }

        /// <summary>
        /// Checks that the song is one of the project and the part one of the song.
        /// A part without a song: the song of the part is used. Returns the song id to save.
        /// </summary>
        private async Task<int?> CheckSongAndSection(int projectId, int? songId, int? sectionId)
        {
            if (sectionId != null)
            {
                var section = await _sectionRepository.GetAsync(sectionId.Value)
                    ?? throw new EntityNotFoundException(nameof(SongSection), sectionId.Value);

                if (songId != null && section.SongId != songId)
                {
                    throw new BusinessRuleException("voiceMemo.sectionOfOtherSong", "The part belongs to another song.");
                }

                songId = section.SongId;
            }

            if (songId != null && !await _songRepository.ExistsAsync(s => s.Id == songId && s.ProjectId == projectId))
            {
                throw new BusinessRuleException("voiceMemo.songOfOtherProject", "The song belongs to another project.");
            }

            return songId;
        }

        /// <summary>
        /// The file name without extension as title, e.g. "Neue Aufnahme 37" (max. 200 characters).
        /// </summary>
        private static string TitleOf(string fileName)
        {
            var title = Path.GetFileNameWithoutExtension(fileName).Trim();
            if (title.Length == 0)
            {
                title = "Memo";
            }

            return title.Length > 200 ? title[..200] : title;
        }
    }
}
