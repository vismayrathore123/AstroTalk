using System;
using System.Collections.Generic;
using System.Text;

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
        public GrahanType GrahanType { get; set; }
        public string SelectedNavgrah { get; set; }
    }

    public enum GrahanType
    {
        None,
        ChandraGrahan,
        SuryaGrahan
    }


    public static class NavgrahList
    {
        public static List<string> Planets = new()
    {
        "Surya (Sun)",
        "Chandra (Moon)",
        "Mangal (Mars)",
        "Budha (Mercury)",
        "Guru (Jupiter)",
        "Shukra (Venus)",
        "Shani (Saturn)",
        "Rahu",
        "Ketu"
    };
    }
}
