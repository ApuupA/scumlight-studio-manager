using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;

namespace StudioManager.ServiceLayer.IService.IProjectServices
{
    /// <summary>
    /// The client page (link without login, same secret as the band gear link): mix versions with feedback,
    /// approving a mix, references. Plus what the studio has not seen yet.
    /// </summary>
    public interface IClientLinkService
    {
        /// <summary>The page for the client, null if the link does not exist (anymore).</summary>
        Task<ClientPageDto?> GetPage(string token);

        /// <summary>The client comments on a mix version. Returns the updated page.</summary>
        Task<ClientPageDto> AddFeedback(string token, ClientFeedbackCreateDto dto);

        /// <summary>The client approves a mix version ("Mix freigeben"). Returns the updated page.</summary>
        Task<ClientPageDto> ApproveVersion(string token, int mixVersionId, ClientApproveDto dto);

        /// <summary>The client adds a reference track. Returns the updated page.</summary>
        Task<ClientPageDto> AddReference(string token, ClientReferenceCreateDto dto);

        /// <summary>For the studio: unseen comments, approvals and references per project, newest first.</summary>
        Task<IEnumerable<ClientNewsDto>> GetNews();

        /// <summary>For the studio: everything the client wrote in this project counts as seen.</summary>
        Task MarkRead(int projectId);
    }
}
