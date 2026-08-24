using System;
using System.Collections.Generic;

namespace AstroDeepak.Domain.Entities
{
    public class RemedyChoice
    {
        public string Name { get; set; } = string.Empty;
        public bool IsPermanent { get; set; }
        public bool IsYearly { get; set; }
    }

    public class GrahRemedySelection
    {
        public int NavgrahId { get; set; }
        public string NavgrahName { get; set; } = string.Empty;
        public List<RemedyChoice> Remedies { get; set; } = new();
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

        public List<GrahRemedySelection> Selections { get; set; } = new();
        public List<string> SelectedPrecautions { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}