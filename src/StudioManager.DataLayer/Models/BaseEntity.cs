using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.Models
{
    /// <summary>
    /// Base class for all database entities. DateCreated / DateUpdated are set automatically in AppDbContext.SaveChanges.
    /// </summary>
    public class BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime DateUpdated { get; set; } = DateTime.UtcNow;
    }
}
