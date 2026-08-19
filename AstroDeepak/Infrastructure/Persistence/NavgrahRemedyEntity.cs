using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace AstroDeepak.Infrastructure.Persistence
{
    [Table("NavgrahRemedy")]
    public class NavgrahRemedyEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string NavgrahName { get; set; }
        public string RemedyName { get; set; }
    }
}
