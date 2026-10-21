using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// The client approves a mix version on the client page ("Mix freigeben").
    /// </summary>
    public class ClientApproveDto
    {
        [Required, MaxLength(100)]
        public string AuthorName { get; set; } = string.Empty;
    }
}
