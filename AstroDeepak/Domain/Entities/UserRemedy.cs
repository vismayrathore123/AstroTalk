using System;

namespace AstroDeepak.Domain.Entities
{
    public class UserRemedy
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public int NavgrahId { get; set; }

        public string CurrentSuggestedRemedy { get; set; } = string.Empty;
        public string RemediesJson { get; set; } = "[]";
        public bool WhatsApp { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}