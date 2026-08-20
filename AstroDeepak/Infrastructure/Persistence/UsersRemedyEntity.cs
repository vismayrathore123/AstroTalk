using SQLite;
using System;

namespace AstroDeepak.Infrastructure.Persistence
{
    [Table("UsersRemedies")]
    public class UsersRemedyEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int PersonId { get; set; }

        [Indexed]
        public int NavgrahId { get; set; }

        public string CurrentSuggestedRemedy { get; set; }
        public string RemediesJson { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}