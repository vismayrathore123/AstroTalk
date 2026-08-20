using System;
using System.Collections.Generic;

namespace AstroDeepak.Domain.Entities
{
    public class UserRemedyStaging
    {
        public int Id { get; set; }

        // 0 when this is a brand-new person, real Id when editing an existing one.
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

        public int NavgrahId { get; set; }
        public string NavgrahName { get; set; }

        public List<string> SelectedRemedies { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}