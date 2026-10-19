using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.ServiceLayer.IService.IProjectServices
{
    /// <summary>
    /// Input lists of a project. Every returned list contains one row per studio input.
    /// </summary>
    public interface IInputListService : IBaseService<InputList, InputListReadDto, InputListCreateDto>
    {
        Task<IEnumerable<InputListReadDto>> GetForProject(int projectId);

        /// <summary>
        /// Saves what is plugged into one studio input. A completely empty row is removed.
        /// Returns the whole updated list.
        /// </summary>
        Task<InputListReadDto> SaveRow(int inputListId, int studioInputId, InputListRowSaveDto dto);

        /// <summary>
        /// Puts the microphones / DIs ("needs an input") of the given instruments into the free inputs.
        /// Entries that are already in the list are skipped.
        /// </summary>
        Task<InputListFillResultDto> FillFromSetups(int inputListId, InputListFillDto dto);

        /// <summary>
        /// Moves source, microphone, stand and +48V to another studio input (e.g. the snare to another preamp).
        /// If the target input is used, the two entries swap places. Returns the whole updated list.
        /// </summary>
        Task<InputListReadDto> MoveEntry(int inputListId, InputListMoveDto dto);

        /// <summary>
        /// Removes all rows - the list is empty again.
        /// </summary>
        Task<InputListReadDto> ClearRows(int inputListId);
    }
}
