using SQLite;
using System;

namespace AstroDeepak.Infrastructure.Persistence
{
    [Table("PrecautionMaster")]
    public class PrecautionMasterEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Text { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}   