using System.ComponentModel.DataAnnotations.Schema;

namespace FarmCraft.Community.Data.Entities
{
    /// <summary>
    /// The base of all FarmCraft tables, containing fields
    /// for the date created, date modified, and
    /// a soft delete
    /// </summary>
    public class FarmCraftBase
    {
        /// <summary>
        /// The date a row / entity was created
        /// </summary>
        [Column("created")]
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// The date a row / entity was last modified
        /// </summary>
        [Column("modified")]
        public DateTimeOffset Modified { get; set; }

        /// <summary>
        /// Whether the row / entity is soft-deleted
        /// </summary>
        [Column("is_deleted")]
        public bool IsDeleted { get; set; }
    }
}
