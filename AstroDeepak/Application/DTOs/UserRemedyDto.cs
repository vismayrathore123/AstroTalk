using System;

namespace AstroDeepak.Application.DTOs
{
    public class UserRemedyDto
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public int NavgrahId { get; set; }
        public string CurrentSuggestedRemedy { get; set; } = string.Empty;
        public string RemediesJson { get; set; } = "[]";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}