using System;

namespace AstroDeepak.Domain.Entities
{
   
    public class PermanentRemedy
    {
        public int Id { get; set; }            // PermanentId
        public int NavgrahId { get; set; }
        public int PersonId { get; set; }      // UserId
        public string RemedyName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}