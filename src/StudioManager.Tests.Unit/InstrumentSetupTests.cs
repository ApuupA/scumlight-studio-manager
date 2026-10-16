using StudioManager.DataLayer.Context;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.DataLayer.Models.GearModels;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.RepositoryLayer.Repository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.Service.ProjectServices;

namespace StudioManager.Tests.Unit
{
    /// <summary>
    /// Tests for instruments, their setup entries and setup templates.
    /// </summary>
    public class InstrumentSetupTests
    {
        private readonly AppDbContext _context = TestHelper.CreateDbContext();
        private readonly ProjectInstrumentService _instrumentService;
        private readonly InstrumentSetupItemService _setupItemService;
        private readonly SetupTemplateService _templateService;

        private readonly Project _project;
        private readonly Project _otherProject;
        private readonly GearItem _sm57;
        private readonly ExternalGear _lesPaul;
        private readonly ExternalGear _otherProjectsAmp;

        public InstrumentSetupTests()
        {
            var mapper = TestHelper.CreateMapper();
            _instrumentService = new ProjectInstrumentService(
                new Repository<ProjectInstrument>(_context), new Repository<Project>(_context), new Repository<SetupTemplate>(_context), mapper);
            _setupItemService = new InstrumentSetupItemService(
                new Repository<InstrumentSetupItem>(_context), new Repository<ProjectInstrument>(_context), new Repository<ExternalGear>(_context), mapper);
            _templateService = new SetupTemplateService(new Repository<SetupTemplate>(_context), new Repository<ProjectInstrument>(_context), mapper);

            var client = new Client { Name = "Label" };
            _project = new Project { Title = "Album", Client = client };
            _otherProject = new Project { Title = "Other", Client = client };
            _sm57 = new GearItem { Name = "SM57", Status = GearStatus.InRepair, GearCategory = new GearCategory { Name = "Microphones" } };
            _lesPaul = new ExternalGear { Name = "Gibson Les Paul", Owner = "Max", Project = _project };
            _otherProjectsAmp = new ExternalGear { Name = "Amp", Project = _otherProject };
            _context.AddRange(_sm57, _lesPaul, _otherProjectsAmp);
            _context.SaveChanges();
        }

        private async Task<int> CreateInstrument(string name = "Guitar") =>
            (await _instrumentService.Insert(new ProjectInstrumentCreateDto { Name = name, ProjectId = _project.Id, IsChain = true })).Id;

        private InstrumentSetupItemCreateDto Item(int instrumentId, int? gearItemId = null, int? externalGearId = null, string? customName = null) => new()
        {
            ProjectInstrumentId = instrumentId,
            GearItemId = gearItemId,
            ExternalGearId = externalGearId,
            CustomName = customName,
            UsedFor = "Cab"
        };

        [Fact]
        public async Task SetupItems_ShouldShowNameAndSourceOfEveryKind()
        {
            var guitar = await CreateInstrument();
            await _setupItemService.Insert(Item(guitar, externalGearId: _lesPaul.Id));
            await _setupItemService.Insert(Item(guitar, customName: "Rented JCM800"));
            await _setupItemService.Insert(Item(guitar, gearItemId: _sm57.Id));

            var instrument = Assert.Single(await _instrumentService.GetForProject(_project.Id));

            Assert.Equal(new[] { "Gibson Les Paul", "Rented JCM800", "SM57" }, instrument.SetupItems.Select(i => i.DisplayName));
            Assert.Equal(new[] { SetupItemSource.Band, SetupItemSource.Custom, SetupItemSource.Studio }, instrument.SetupItems.Select(i => i.Source));
            Assert.Equal(new[] { 1, 2, 3 }, instrument.SetupItems.Select(i => i.SortOrder)); // appended in order
            Assert.Equal(GearStatus.InRepair, instrument.SetupItems[2].GearStatus);           // frontend warns
            Assert.Equal("Max", instrument.SetupItems[0].ExternalGearOwner);
        }

        [Theory]
        [InlineData(false, false, null)]   // no source
        [InlineData(true, false, "Amp")]   // two sources
        [InlineData(false, false, "   ")]  // only spaces = no source
        public async Task SetupItem_ShouldNeedExactlyOneSource(bool withGear, bool withBandGear, string? customName)
        {
            var guitar = await CreateInstrument();
            var dto = Item(guitar, withGear ? _sm57.Id : null, withBandGear ? _lesPaul.Id : null, customName);

            var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _setupItemService.Insert(dto));

            Assert.Equal("setupItem.oneSourceRequired", exception.ErrorCode);
        }

        [Fact]
        public async Task SetupItem_ShouldRejectBandGearOfAnotherProject()
        {
            var guitar = await CreateInstrument();

            var exception = await Assert.ThrowsAsync<BusinessRuleException>(
                () => _setupItemService.Insert(Item(guitar, externalGearId: _otherProjectsAmp.Id)));

            Assert.Equal("setupItem.bandGearNotInProject", exception.ErrorCode);
        }

        [Fact]
        public async Task Move_ShouldSwapWithTheNeighbour()
        {
            var guitar = await CreateInstrument();
            var first = await _setupItemService.Insert(Item(guitar, customName: "Guitar"));
            await _setupItemService.Insert(Item(guitar, customName: "Amp"));
            var third = await _setupItemService.Insert(Item(guitar, customName: "Cab"));

            await _setupItemService.Move(third.Id, new SetupItemMoveDto { Direction = -1 });
            await _setupItemService.Move(first.Id, new SetupItemMoveDto { Direction = -1 }); // already first: no change

            var instrument = (await _instrumentService.Get(guitar))!;
            Assert.Equal(new[] { "Guitar", "Cab", "Amp" }, instrument.SetupItems.Select(i => i.DisplayName));
        }

        [Fact]
        public async Task Template_ShouldLeaveOutBandGear_AndCreateANewInstrument()
        {
            var guitar = await CreateInstrument();
            await _setupItemService.Insert(Item(guitar, externalGearId: _lesPaul.Id));
            await _setupItemService.Insert(Item(guitar, gearItemId: _sm57.Id));

            var template = await _templateService.CreateFromInstrument(new SetupTemplateFromInstrumentDto { ProjectInstrumentId = guitar, Name = "Guitar standard" });
            var applied = await _instrumentService.ApplyTemplate(new ApplySetupTemplateDto { ProjectId = _project.Id, SetupTemplateId = template.Id });

            Assert.Equal("SM57", Assert.Single(template.Items).DisplayName);
            Assert.True(template.IsChain);
            Assert.Equal("Guitar standard", applied.Name);
            Assert.Equal("SM57", Assert.Single(applied.SetupItems).DisplayName);
            Assert.Equal(2, (await _instrumentService.GetForProject(_project.Id)).Count());
        }

        [Fact]
        public async Task GearUsages_ShouldListProjectAndInstrument()
        {
            var drums = await CreateInstrument("Drums");
            await _setupItemService.Insert(Item(drums, gearItemId: _sm57.Id));

            var usage = Assert.Single(await _setupItemService.GetUsagesOfGearItem(_sm57.Id));

            Assert.Equal("Album", usage.ProjectTitle);
            Assert.Equal("Drums", usage.InstrumentName);
            Assert.Equal("Cab", usage.UsedFor);
        }
    }
}
