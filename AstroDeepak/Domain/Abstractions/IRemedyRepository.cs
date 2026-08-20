using AstroDeepak.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AstroDeepak.Domain.Abstractions
{
    public interface IRemedyRepository
    {
        Task<List<RemedyMaster>> GetAllRemediesAsync();
        Task<List<string>> GetRemediesForNavgrahAsync(string navgrahName);
        Task SaveNavgrahRemediesAsync(string navgrahName, List<string> remedyNames);
        Task AddRemedyMasterAsync(string name);
        Task DeleteRemedyMasterAsync(int id);

    }
}
