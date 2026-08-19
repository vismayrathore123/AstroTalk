using System;

namespace AstroDeepak.Application.DTOs
{
    public class PersonDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FatherName { get; set; }
        public string Gotra { get; set; }
        public DateTime? DOB { get; set; }
        public string Time { get; set; }
        public string BirthPlace { get; set; }
        public string PhoneNo { get; set; }
        public string Address { get; set; }

        // "None", "Surya", "Chandra" ... one of the 9 Navgrah names.
        public string SelectedGrah { get; set; } = "None";

        public DateTime CreatedAt { get; set; }

        // Convenience property shown in the search / recent list rows.
        public string Subtitle => $"{BirthPlace}  •  {(DOB.HasValue ? DOB.Value.ToString("dd MMM yyyy") : "DOB not set")}";
    }
}
