using SQLite;
using System;

namespace AstroDeepak.Infrastructure.Persistence
{
    [Table("UserRemedyStaging")]
    public class UserRemedyStagingEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int PersonId { get; set; }

        public string Name { get; set; }
        public string FatherName { get; set; }
        public string Gotra { get; set; }
        public DateTime DOB { get; set; }
        public string Time { get; set; }
        public string BirthPlace { get; set; }
        public string CountryCode { get; set; }
        public string PhoneNo { get; set; }
        public string Address { get; set; }
        public string Grahan { get; set; }

        // JSON-serialized List<GrahRemedySelection> - one entry per Grah picked in
        // this session, each with its own remedy list.
        public string SelectionsJson { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}