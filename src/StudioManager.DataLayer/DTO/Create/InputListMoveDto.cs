using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// Moves what is plugged in (source, microphone, stand) from one studio input to another,
    /// e.g. the snare to another preamp. If the target is used, the two entries swap places.
    /// </summary>
    public class InputListMoveDto
    {
        [Range(1, int.MaxValue)]
        public int FromStudioInputId { get; set; }

        [Range(1, int.MaxValue)]
        public int ToStudioInputId { get; set; }
    }
}
