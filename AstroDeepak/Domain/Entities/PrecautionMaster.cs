using System;

namespace AstroDeepak.Domain.Entities
{
    public class PrecautionMaster
    {
        public int Id { get; set; }          // PrecautionId
        public string Text { get; set; } = string.Empty;  // PrecautionText
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}