using System;
using System.Collections.Generic;

namespace AstroDeepak.Application.DTOs
{
    public class GrahRemedySelectionDto
    {
        public int NavgrahId { get; set; }
        public string NavgrahName { get; set; } = string.Empty;
        public List<string> Remedies { get; set; } = new();
    }

    public class UserRemedyStagingDto
    {
        public int Id { get; set; }
        public int PersonId { get; set; }

        public string Name { get; set; }
        public string FatherName { get; set; }
        public string Gotra { get; set; }
        public DateTime DOB { get; set; }
        public string Time { get; set; }
        public string BirthPlace { get; set; }
        public string PhoneNo { get; set; }
        public string Address { get; set; }
        public string Grahan { get; set; }
        public string CountryCode { get; set; }

        public List<GrahRemedySelectionDto> Selections { get; set; } = new();
    }
}