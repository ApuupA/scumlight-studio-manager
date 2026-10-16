namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// "Fill from setups": the instruments whose microphones / DIs are put into the free inputs of the list.
    /// </summary>
    public class InputListFillDto
    {
        public List<int> InstrumentIds { get; set; } = new();
    }
}
