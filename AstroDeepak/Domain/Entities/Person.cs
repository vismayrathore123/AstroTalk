using System;

namespace AstroDeepak.Domain.Entities
{
    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FatherName { get; set; }
        public string Gotra { get; set; }
        public DateTime DOB { get; set; }
        public string Time { get; set; }
        public string BirthPlace { get; set; }
        public string PhoneNo { get; set; }
        public string Address { get; set; }

        // Now free-text names coming from NavgrahMaster / GrahanMaster tables
        // instead of a fixed enum.
        public string SelectedGrah { get; set; } = "None";
        public string SelectedRemedies { get; set; } = string.Empty;
        public string RemediesJson { get; set; } = "[]";

        public string SelectedGrahan { get; set; } = "None";
        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }


}
