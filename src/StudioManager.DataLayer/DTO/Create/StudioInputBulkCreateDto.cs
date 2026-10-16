using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// Creates several studio inputs at once, numbered automatically. Example: Count 8, SnakePrefix "16CH - ",
    /// SnakeStart 9, PreampPrefix "SSL - ", PreampStart 1, DawPrefix "Apollo 16 - ", DawStart 1
    /// -> "16CH - 9 / SSL - 1 / Apollo 16 - 1" up to "16CH - 16 / SSL - 8 / Apollo 16 - 8".
    /// Without a start number the prefix is used as it is for every input (e.g. Preamp "UNISON").
    /// </summary>
    public class StudioInputBulkCreateDto
    {
        [Range(1, 64)]
        public int Count { get; set; } = 8;

        [MaxLength(40)]
        public string? SnakePrefix { get; set; }
        public int? SnakeStart { get; set; }

        [MaxLength(90)]
        public string? PreampPrefix { get; set; }
        public int? PreampStart { get; set; }

        [MaxLength(40)]
        public string? DawPrefix { get; set; }
        public int? DawStart { get; set; }

        public int? InputGroupId { get; set; }
    }
}
