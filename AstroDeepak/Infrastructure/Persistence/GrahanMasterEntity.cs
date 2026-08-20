using SQLite;
using System;

namespace AstroDeepak.Infrastructure.Persistence
{
    [Table("GrahanMaster")]
    public class GrahanMasterEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}