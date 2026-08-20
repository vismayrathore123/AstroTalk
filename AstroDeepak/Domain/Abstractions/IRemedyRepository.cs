using AstroDeepak.Domain.Entities;

namespace AstroDeepak.Domain.Abstractions
{
    public interface IRemedyRepository
    {
        Task<List<RemedyMaster>> GetRemediesByNavgrahIdAsync(int navgrahId);
        Task AddRemedyMasterAsync(string name, int navgrahId);
        Task UpdateRemedyMasterAsync(int id, string name);
        Task DeleteRemedyMasterAsync(int id);
    }
}