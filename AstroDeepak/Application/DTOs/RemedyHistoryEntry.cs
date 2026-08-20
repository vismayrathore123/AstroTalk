using System;
using System.Collections.Generic;
using System.Text;

namespace AstroDeepak.Application.DTOs
{
    public class RemedyHistoryEntry
    {
        public DateTime CreatedAt { get; set; }
        public List<string> Remedies { get; set; } = new();
    }
}
