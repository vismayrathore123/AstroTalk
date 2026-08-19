using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace AstroDeepak.Infrastructure.Persistence
{
    [Table("RemedyMaster")]
    public class RemedyMasterEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public int SortOrder { get; set; }
    }
}
