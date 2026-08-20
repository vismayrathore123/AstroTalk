using System;

namespace AstroDeepak.Domain.Entities
{
    public class RemedyMaster
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int NavgrahId { get; set; }

        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}