using System;

namespace AstroDeepak.Domain.Entities
{
    public class PrecautionMaster
    {
        public int Id { get; set; }          
        public string Text { get; set; } = string.Empty;  
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}   