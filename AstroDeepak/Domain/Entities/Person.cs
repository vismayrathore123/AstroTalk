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

        // Renamed: SelectedGrah -> Grah, SelectedGrahan -> Grahan
        public string Grah { get; set; } = "None";
        public string Grahan { get; set; } = "None";
        public string CountryCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}