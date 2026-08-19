using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace AstroDeepak.Infrastructure.Persistence
{
    [Table("NavgrahMaster")]
    public class NavgrahMasterEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public int SortOrder { get; set; }
    }
}
