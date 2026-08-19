using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace AstroDeepak.Infrastructure.Persistence
{
    [Table("GrahanMaster")]
    public class GrahanMasterEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public int SortOrder { get; set; }
    }
}
