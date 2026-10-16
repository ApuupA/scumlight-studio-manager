using StudioManager.DataLayer.Context;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.DataLayer.Models.GearModels;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.DataLayer.Models.SettingsModels;
using StudioManager.RepositoryLayer.Repository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.Service.ProjectServices;
using StudioManager.ServiceLayer.Service.SettingsServices;

namespace StudioManager.Tests.Unit
{
    /// <summary>
    /// Tests for studio inputs and input lists.
    /// </summary>
    public class InputListTests
    {
        private readonly AppDbContext _context = TestHelper.CreateDbContext();
        private readonly StudioInputService _studioInputService;
        private readonly InputListService _inputListService;
        private readonly Project _project;
        private readonly ProjectInstrument _drums;

        public InputListTests()
        {
            var mapper = TestHelper.CreateMapper();
            _studioInputService = new StudioInputService(new Repository<StudioInput>(_context), mapper);
            _inputListService = new InputListService(
                new Repository<InputList>(_context),
                new Repository<StudioInput>(_context),
                new Repository<InputListRow>(_context),
                new Repository<ProjectInstrument>(_context),
                mapper);

            _project = new Project { Title = "Album", Client = new Client { Name = "Label" } };
            var mics = new GearCategory { Name = "Microphones" };
            _drums = new ProjectInstrument
            {
                Name = "Drums",
                Project = _project,
                SetupItems =
                [
                    new InstrumentSetupItem { GearItem = new GearItem { Name = "D6", GearCategory = mics }, UsedFor = "Kick In", NeedsInput = true, SortOrder = 1 },
                    new InstrumentSetupItem { CustomName = "Drum rug", NeedsInput = false, SortOrder = 2 },
                    new InstrumentSetupItem { GearItem = new GearItem { Name = "C414", GearCategory = mics }, UsedFor = "OH", Quantity = 2, NeedsInput = true, SortOrder = 3 }
                ]
            };
            _context.ProjectInstruments.Add(_drums);
            _context.SaveChanges();
        }

        private async Task<int> CreateList() =>
            (await _inputListService.Insert(new InputListCreateDto { Name = "Drums", ProjectId = _project.Id })).Id;

        [Fact]
        public async Task BulkCreate_ShouldNumberTheChannels()
        {
            var inputs = (await _studioInputService.BulkCreate(new StudioInputBulkCreateDto
            {
                Count = 3,
                SnakePrefix = "16CH - ", SnakeStart = 9,
                PreampPrefix = "UNISON",            // no start number: same text for every input
                DawPrefix = "Apollo 16 - ", DawStart = 1
            })).ToList();

            Assert.Equal(new[] { "16CH - 9", "16CH - 10", "16CH - 11" }, inputs.Select(i => i.SnakeChannel));
            Assert.All(inputs, i => Assert.Equal("UNISON", i.Preamp));
            Assert.Equal(new[] { "Apollo 16 - 1", "Apollo 16 - 2", "Apollo 16 - 3" }, inputs.Select(i => i.DawChannel));
            Assert.Equal(new[] { 1, 2, 3 }, inputs.Select(i => i.SortOrder));
        }

        [Fact]
        public async Task Get_ShouldReturnOneRowPerStudioInput()
        {
            await _studioInputService.BulkCreate(new StudioInputBulkCreateDto { Count = 4, DawPrefix = "In ", DawStart = 1 });
            var listId = await CreateList();
            var secondInputId = _context.StudioInputs.OrderBy(i => i.SortOrder).Skip(1).First().Id;

            var list = await _inputListService.SaveRow(listId, secondInputId, new InputListRowSaveDto { Source = " Snare Top ", Microphone = "SM57" });

            Assert.Equal(4, list.Rows.Count);
            Assert.Equal(new[] { 1, 2, 3, 4 }, list.Rows.Select(r => r.Number));
            Assert.Equal("Snare Top", list.Rows[1].Source); // trimmed
            Assert.Null(list.Rows[0].RowId);                 // unused input
            Assert.Equal(1, list.UsedInputCount);
        }

        [Fact]
        public async Task SaveRow_WithEmptyValues_ShouldFreeTheInput()
        {
            await _studioInputService.BulkCreate(new StudioInputBulkCreateDto { Count = 1 });
            var listId = await CreateList();
            var inputId = _context.StudioInputs.Single().Id;
            await _inputListService.SaveRow(listId, inputId, new InputListRowSaveDto { Source = "Vocals", Phantom = true });

            var list = await _inputListService.SaveRow(listId, inputId, new InputListRowSaveDto { Source = "  " });

            Assert.Equal(0, list.UsedInputCount);
            Assert.Empty(_context.InputListRows);
        }

        [Fact]
        public async Task FillFromSetups_ShouldPutMicsIntoFreeInputs_AndSkipExistingOnes()
        {
            await _studioInputService.BulkCreate(new StudioInputBulkCreateDto { Count = 5, DawPrefix = "In ", DawStart = 1 });
            var listId = await CreateList();
            var firstInputId = _context.StudioInputs.OrderBy(i => i.SortOrder).First().Id;
            await _inputListService.SaveRow(listId, firstInputId, new InputListRowSaveDto { Source = "Talkback" }); // input 1 is taken

            var result = await _inputListService.FillFromSetups(listId, new InputListFillDto { InstrumentIds = [_drums.Id] });
            var again = await _inputListService.FillFromSetups(listId, new InputListFillDto { InstrumentIds = [_drums.Id] });

            Assert.Equal(3, result.AddedCount); // D6 + 2x C414, the drum rug needs no input
            Assert.Equal(0, result.NotPlacedCount);
            Assert.Equal(
                new[] { "Talkback", "Kick In", "OH 1", "OH 2", null },
                result.InputList.Rows.Select(r => r.Source));
            Assert.Equal("C414", result.InputList.Rows[2].Microphone);
            Assert.Equal(0, again.AddedCount); // nothing new - already in the list
        }

        [Fact]
        public async Task FillFromSetups_ShouldCountMicsThatDoNotFit()
        {
            await _studioInputService.BulkCreate(new StudioInputBulkCreateDto { Count = 2 });
            var listId = await CreateList();

            var result = await _inputListService.FillFromSetups(listId, new InputListFillDto { InstrumentIds = [_drums.Id] });

            Assert.Equal(2, result.AddedCount);
            Assert.Equal(1, result.NotPlacedCount);
        }

        [Fact]
        public async Task FillFromSetups_ShouldRejectInstrumentsOfAnotherProject()
        {
            var foreign = new ProjectInstrument { Name = "Other", Project = new Project { Title = "Other", ClientId = _project.ClientId } };
            _context.ProjectInstruments.Add(foreign);
            await _context.SaveChangesAsync();
            var listId = await CreateList();

            var exception = await Assert.ThrowsAsync<BusinessRuleException>(
                () => _inputListService.FillFromSetups(listId, new InputListFillDto { InstrumentIds = [foreign.Id] }));

            Assert.Equal("inputList.instrumentNotInProject", exception.ErrorCode);
        }
    }
}
