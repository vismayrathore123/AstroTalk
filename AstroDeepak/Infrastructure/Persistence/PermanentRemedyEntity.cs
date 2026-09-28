using SQLite;
using System;

namespace AstroDeepak.Infrastructure.Persistence
{
    [Table("PermanentRemedy")]
    public class PermanentRemedyEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }       
        [Indexed]
        public int NavgrahId { get; set; }
        [Indexed]
        public int PersonId { get; set; }  
        public string RemedyName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}