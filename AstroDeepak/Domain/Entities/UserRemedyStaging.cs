using System;
using System.Collections.Generic;

namespace AstroDeepak.Domain.Entities
{
    public class GrahRemedySelection
    {
        public int NavgrahId { get; set; }
        public string NavgrahName { get; set; } = string.Empty;
        public List<string> Remedies { get; set; } = new();
    }

    public class UserRemedyStaging
    {
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

        // One staging row now carries every Grah + remedies picked in this session,
        // instead of a single Grah at a time.
        public List<GrahRemedySelection> Selections { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}