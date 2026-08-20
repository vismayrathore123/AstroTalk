using SQLite;
using System;

namespace AstroDeepak.Infrastructure.Persistence
{
    [Table("RemedyMaster")]
    public class RemedyMasterEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public int SortOrder { get; set; }

        [Indexed]
        public int NavgrahId { get; set; } // FK -> NavgrahMaster.Id

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}