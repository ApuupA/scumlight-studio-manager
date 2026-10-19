using StudioManager.DataLayer.Context;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.RepositoryLayer.Repository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.Service.ProjectServices;

namespace StudioManager.Tests.Unit
{
    /// <summary>
    /// Tests for the band gear link: the band enters gear without login, the studio accepts it.
    /// </summary>
    public class BandGearLinkTests
    {
        private readonly AppDbContext _context = TestHelper.CreateDbContext();
        private readonly BandGearLinkService _linkService;
        private readonly ExternalGearService _gearService;
        private readonly InstrumentSetupItemService _setupItemService;
        private readonly Project _project;

        public BandGearLinkTests()
        {
            var mapper = TestHelper.CreateMapper();
            _linkService = new BandGearLinkService(new Repository<Project>(_context), new Repository<ExternalGear>(_context));
            _gearService = new ExternalGearService(new Repository<ExternalGear>(_context), mapper);
            _setupItemService = new InstrumentSetupItemService(
                new Repository<InstrumentSetupItem>(_context), new Repository<ProjectInstrument>(_context), new Repository<ExternalGear>(_context), mapper);

            var label = new Client { Name = "Label" };
            _project = new Project { Title = "Album", Client = label, Bands = [new Band { Name = "Sacroscum", Client = label }] };
            _context.Projects.Add(_project);
            _context.SaveChanges();
        }

        private async Task<string> CreateLink() => (await _linkService.CreateLink(_project.Id)).Token!;

        private static BandGearSuggestionDto Gear(string name, string? owner = null) => new() { Name = name, Owner = owner };

        [Fact]
        public async Task Band_ShouldSeeTheProject_AndItsSuggestions()
        {
            var token = await CreateLink();

            var form = await _linkService.AddSuggestion(token, Gear(" Les Paul ", "Max"));

            Assert.Equal("Album", form.ProjectTitle);
            Assert.Equal("Sacroscum", form.BandNames);
            var entry = Assert.Single(form.Entries);
            Assert.Equal("Les Paul", entry.Name); // trimmed
            Assert.True(entry.IsSuggestion);
        }

        [Fact]
        public async Task Accept_ShouldTurnASuggestionIntoBandGear()
        {
            var token = await CreateLink();
            var entry = (await _linkService.AddSuggestion(token, Gear("Ampeg SVT"))).Entries.Single();

            var accepted = await _gearService.Accept(entry.Id);

            Assert.False(accepted.IsSuggestion);
        }

        [Fact]
        public async Task Band_ShouldOnlyDeleteSuggestions_NotAcceptedGear()
        {
            var token = await CreateLink();
            var form = await _linkService.AddSuggestion(token, Gear("Cymbals"));
            form = await _linkService.AddSuggestion(token, Gear("Snare"));
            var cymbals = form.Entries.Single(e => e.Name == "Cymbals");
            var snare = form.Entries.Single(e => e.Name == "Snare");
            await _gearService.Accept(snare.Id);

            var afterDelete = await _linkService.DeleteSuggestion(token, cymbals.Id);

            Assert.Equal("Snare", Assert.Single(afterDelete.Entries).Name);
            await Assert.ThrowsAsync<EntityNotFoundException>(() => _linkService.DeleteSuggestion(token, snare.Id));
        }

        [Fact]
        public async Task NewOrDeletedLink_ShouldMakeTheOldOneInvalid()
        {
            var oldToken = await CreateLink();
            var newToken = await CreateLink();

            Assert.Null(await _linkService.GetForm(oldToken));
            Assert.NotNull(await _linkService.GetForm(newToken));
            await Assert.ThrowsAsync<EntityNotFoundException>(() => _linkService.AddSuggestion(oldToken, Gear("Bass")));

            await _linkService.DeleteLink(_project.Id);

            Assert.Null(await _linkService.GetForm(newToken));
            Assert.Null((await _linkService.GetLink(_project.Id)).Token);
        }

        [Fact]
        public async Task TooManyOpenSuggestions_ShouldBeRejected()
        {
            var token = await CreateLink();
            for (var i = 0; i < BandGearLinkService.MaxOpenSuggestions; i++)
            {
                _context.ExternalGear.Add(new ExternalGear { Name = $"Item {i}", ProjectId = _project.Id, IsSuggestion = true });
            }
            await _context.SaveChangesAsync();

            var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _linkService.AddSuggestion(token, Gear("One more")));

            Assert.Equal("bandGearLink.tooManySuggestions", exception.ErrorCode);
        }

        [Fact]
        public async Task Setup_ShouldNotUseASuggestion_BeforeItIsAccepted()
        {
            var token = await CreateLink();
            var entry = (await _linkService.AddSuggestion(token, Gear("Marshall JCM800"))).Entries.Single();
            var instrument = new ProjectInstrument { Name = "Guitar", ProjectId = _project.Id };
            _context.ProjectInstruments.Add(instrument);
            await _context.SaveChangesAsync();
            var item = new InstrumentSetupItemCreateDto { ProjectInstrumentId = instrument.Id, ExternalGearId = entry.Id };

            var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _setupItemService.Insert(item));
            await _gearService.Accept(entry.Id);
            var created = await _setupItemService.Insert(item);

            Assert.Equal("setupItem.bandGearNotAccepted", exception.ErrorCode);
            Assert.Equal("Marshall JCM800", created.DisplayName);
        }
    }
}
