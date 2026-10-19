using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;

namespace StudioManager.ServiceLayer.IService.IProjectServices
{
    /// <summary>
    /// The band gear link of a project: a public page (no login) where the band enters the gear it brings.
    /// Entries become suggestions - the studio accepts them in the project (ExternalGearService.Accept).
    /// </summary>
    public interface IBandGearLinkService
    {
        // ---------- For the studio (logged in) ----------

        Task<BandGearLinkDto> GetLink(int projectId);

        /// <summary>Creates a new link - an old one stops working.</summary>
        Task<BandGearLinkDto> CreateLink(int projectId);

        /// <summary>Switches the link off.</summary>
        Task DeleteLink(int projectId);

        // ---------- For the band (public page, the link is the key) ----------

        /// <summary>Project and gear for the public page, null if the link is unknown.</summary>
        Task<BandGearFormDto?> GetForm(string token);

        /// <summary>Adds a piece of gear as suggestion. Returns the updated page.</summary>
        Task<BandGearFormDto> AddSuggestion(string token, BandGearSuggestionDto dto);

        /// <summary>Removes a suggestion that was not accepted yet (the band corrects a mistake).</summary>
        Task<BandGearFormDto> DeleteSuggestion(string token, int externalGearId);
    }
}
