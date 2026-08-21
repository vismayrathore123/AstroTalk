using SQLite;
using System;

namespace AstroDeepak.Infrastructure.Persistence
{
    [Table("Persons")]
    public class PersonEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string FatherName { get; set; }
        public string Gotra { get; set; }
        public DateTime DOB { get; set; }
        public string Time { get; set; }
        public string BirthPlace { get; set; }
        public string CountryCode { get; set; }
        public string PhoneNo { get; set; }
        public string Address { get; set; }

        public string Grah { get; set; }
        public string Grahan { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}