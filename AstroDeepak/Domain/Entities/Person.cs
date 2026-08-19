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

        // Replaces the old 2-value GrahanType. This now stores exactly ONE
        // of the 9 Navgrah (planets) the user picks on the NavgrahListPage.
        public NavgrahType SelectedGrah { get; set; }

        // Used to sort the "recent added" list on the Search page.
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // The 9 Navgrahas of Vedic astrology. "None" = not selected yet.
    public enum NavgrahType
    {
        None,
        Surya,
        Chandra,
        Mangal,
        Budh,
        Guru,
        Shukra,
        Shani,
        Rahu,
        Ketu
    }
}
