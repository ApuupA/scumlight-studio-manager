using StudioManager.DataLayer.Context;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.DataLayer.Models.UserModels;
using StudioManager.RepositoryLayer.Repository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.Service.ProjectServices;

namespace StudioManager.Tests.Unit
{
    /// <summary>
    /// Tests for the client page: comments with name, approving a mix, references, "new from the client".
    /// </summary>
    public class ClientLinkTests
    {
        private readonly AppDbContext _context = TestHelper.CreateDbContext();
        private readonly ClientLinkService _service;
        private readonly MixVersionService _versionService;
        private readonly MixFeedbackService _feedbackService;
        private readonly Project _project;
        private readonly Song _opener;
        private readonly Song _ballad;
        private readonly int _versionId;
        private const string Token = "secret-token";

        public ClientLinkTests()
        {
            var mapper = TestHelper.CreateMapper();
            _versionService = new MixVersionService(new Repository<MixVersion>(_context), new Repository<Project>(_context), new Repository<Song>(_context), mapper);
            _feedbackService = new MixFeedbackService(new Repository<MixFeedback>(_context), new Repository<MixVersion>(_context), new Repository<User>(_context), mapper);
            _service = new ClientLinkService(
                new Repository<Project>(_context), new Repository<Song>(_context), new Repository<MixVersion>(_context),
                new Repository<MixFeedback>(_context), new Repository<ProjectLink>(_context), _versionService);

            _project = new Project
            {
                Title = "Debut", BandGearToken = Token, IncludesRecording = false,
                Client = new Client { Name = "Sacroscum" }, Bands = [new Band { Name = "Sacroscum" }]
            };
            _opener = new Song { Title = "Opener", TrackNumber = 1, Project = _project };
            _ballad = new Song { Title = "Ballad", TrackNumber = 2, Project = _project };
            _context.Songs.AddRange(_opener, _ballad);
            _context.Users.Add(new User { Email = "owner@studio.local", DisplayName = "Justin", PasswordHash = "x" });
            _context.SaveChanges();

            _versionId = _versionService.Insert(new MixVersionCreateDto
            {
                ProjectId = _project.Id, Date = new DateOnly(2026, 10, 22), SongIds = [_opener.Id, _ballad.Id], Link = "https://cloud.example/s/v1"
            }).Result.Id;
        }

        [Fact]
        public async Task GetPage_ShouldShowTheVersions_OnlyForTheRightLink()
        {
            Assert.Null(await _service.GetPage("wrong"));

            var page = (await _service.GetPage(Token))!;

            Assert.Equal("Debut", page.ProjectTitle);
            Assert.Equal("Sacroscum", page.BandNames);
            Assert.True(page.ShowMixing);
            Assert.False(page.ShowGear); // no recording in this project
            var version = Assert.Single(page.MixVersions);
            Assert.Equal("v1", version.Name);
            Assert.True(version.IsAllSongs);
            Assert.Equal(["Opener", "Ballad"], page.Songs.Select(s => s.Title));
        }

        [Fact]
        public async Task ClientFeedback_ShouldHaveTheName_AndBeNewForTheStudio()
        {
            var page = await _service.AddFeedback(Token, new ClientFeedbackCreateDto
            {
                MixVersionId = _versionId, SongId = _ballad.Id, Timecode = " 1:23 ", Text = " Vocals too quiet ", AuthorName = " Simon "
            });

            var feedback = Assert.Single(Assert.Single(page.MixVersions).Feedback);
            Assert.Equal("Simon", feedback.Author);
            Assert.Equal("Ballad", feedback.SongTitle);
            Assert.True(feedback.IsFromClient);

            var news = Assert.Single(await _service.GetNews());
            Assert.Equal(1, news.FeedbackCount);
            Assert.Equal(["Simon"], news.Authors);

            await _service.MarkRead(_project.Id);
            Assert.Empty(await _service.GetNews());
        }

        [Fact]
        public async Task ClientFeedback_ShouldOnlyBeForVersionsAndSongsOfTheProject()
        {
            var otherProject = new Project { Title = "Other", Client = new Client { Name = "X" } };
            var otherSong = new Song { Title = "Foreign", Project = otherProject };
            _context.Songs.Add(otherSong);
            await _context.SaveChangesAsync();
            var otherVersion = await _versionService.Insert(new MixVersionCreateDto { ProjectId = otherProject.Id, Date = new DateOnly(2026, 10, 22), SongIds = [otherSong.Id] });

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.AddFeedback(Token,
                new ClientFeedbackCreateDto { MixVersionId = otherVersion.Id, Text = "Hi", AuthorName = "Simon" }));
            var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.AddFeedback(Token,
                new ClientFeedbackCreateDto { MixVersionId = _versionId, SongId = otherSong.Id, Text = "Hi", AuthorName = "Simon" }));
            Assert.Equal("mixFeedback.songNotInVersion", exception.ErrorCode);
        }

        [Fact]
        public async Task Approve_ShouldSetTheStatus_WithTheName_AndAStudioChangeTakesItBack()
        {
            var page = await _service.ApproveVersion(Token, _versionId, new ClientApproveDto { AuthorName = "Simon" });

            var version = Assert.Single(page.MixVersions);
            Assert.Equal(MixVersionStatus.Approved, version.Status);
            Assert.Equal("Simon", version.ApprovedBy);
            Assert.Equal(ClientLinkService.ApprovedText, Assert.Single(version.Feedback).Text);

            // The studio sets the status back - "approved by Simon" goes away
            var updated = await _versionService.Update(_versionId, new MixVersionCreateDto
            {
                ProjectId = _project.Id, Name = "v1", Date = new DateOnly(2026, 10, 22), Status = MixVersionStatus.ChangesRequested, SongIds = [_opener.Id]
            });
            Assert.Null(updated.ApprovedBy);
        }

        [Fact]
        public async Task AddReference_ShouldBeAReferenceLinkWithTheName()
        {
            var page = await _service.AddReference(Token, new ClientReferenceCreateDto
            {
                Title = "Mastodon - Blood and Thunder", Url = "https://open.spotify.com/track/1", SongId = _opener.Id, AuthorName = "Lena"
            });

            var reference = Assert.Single(page.References);
            Assert.Equal("Lena", reference.AddedBy);
            Assert.Equal("Opener", reference.SongTitle);
            var link = _context.ProjectLinks.Single();
            Assert.Equal(ProjectLinkType.Reference, link.Type);
            Assert.True(link.IsUnread);
            Assert.Equal(1, Assert.Single(await _service.GetNews()).ReferenceCount);
        }

        [Fact]
        public async Task StudioComment_ShouldGetTheNameOfTheLoggedInUser()
        {
            var userId = _context.Users.Single().Id;

            var feedback = await _feedbackService.InsertFromStudio(new MixFeedbackCreateDto { MixVersionId = _versionId, Text = "Done in v2" }, userId);

            Assert.Equal("Justin", feedback.Author);
            Assert.False(feedback.IsFromClient);
            Assert.False(feedback.IsUnread);
        }
    }
}
