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

        public string SelectedGrahan { get; set; } = "None";
        public string SelectedGrah { get; set; } = "None";
        public string SelectedRemedies { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public string Subtitle => $"{BirthPlace}  •  {(DOB.HasValue ? DOB.Value.ToString("dd MMM yyyy") : "DOB not set")}";
    }
}