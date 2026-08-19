using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AstroDeepak.Infrastructure.Persistence
{
    [System.ComponentModel.DataAnnotations.Schema.Table("Persons")]
    public class PersonEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string FatherName { get; set; }
        public string Gotra { get; set; }
        public DateTime DOB { get; set; }
        public string Time { get; set; }
        public string BirthPlace { get; set; }
        public string PhoneNo { get; set; }
        public string Address { get; set; }
        public string GrahanType { get; set; }
        public string SelectedNavgrah { get; set; }

    }
}
