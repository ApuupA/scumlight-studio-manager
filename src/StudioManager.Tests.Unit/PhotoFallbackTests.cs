using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ClientModels;

namespace StudioManager.Tests.Unit
{
    /// <summary>
    /// Client and band show each other's picture if they have none of their own.
    /// </summary>
    public class PhotoFallbackTests
    {
        private readonly AutoMapper.IMapper _mapper = TestHelper.CreateMapper();

        [Fact]
        public void BandWithoutPhoto_ShouldShowTheClientPhoto()
        {
            var client = new Client { Id = 1, Name = "Sacroscum", PhotoPath = "client-photos/a.jpg" };
            var band = new Band { Id = 2, Name = "Sacroscum", Client = client };

            var dto = _mapper.Map<BandReadDto>(band);

            Assert.False(dto.HasPhoto);
            Assert.Equal(1, dto.FallbackPhoto?.Id);
        }

        [Fact]
        public void ClientWithoutPhoto_ShouldShowThePhotoOfItsOnlyBand()
        {
            var client = new Client { Id = 1, Name = "Sacroscum" };
            client.Bands.Add(new Band { Id = 2, Name = "Sacroscum", PhotoPath = "band-photos/b.jpg" });

            Assert.Equal(2, _mapper.Map<ClientReadDto>(client).FallbackPhoto?.Id);

            // With two bands it is not clear which picture fits
            client.Bands.Add(new Band { Id = 3, Name = "Side project", PhotoPath = "band-photos/c.jpg" });
            Assert.Null(_mapper.Map<ClientReadDto>(client).FallbackPhoto);
        }

        [Fact]
        public void OwnPhoto_ShouldWin()
        {
            var client = new Client { Id = 1, Name = "Sacroscum", PhotoPath = "client-photos/a.jpg" };
            client.Bands.Add(new Band { Id = 2, Name = "Sacroscum", PhotoPath = "band-photos/b.jpg", Client = client });

            Assert.Null(_mapper.Map<ClientReadDto>(client).FallbackPhoto);
            Assert.Null(_mapper.Map<BandReadDto>(client.Bands.First()).FallbackPhoto);
        }
    }
}
