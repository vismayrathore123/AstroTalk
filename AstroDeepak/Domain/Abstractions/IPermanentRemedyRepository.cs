using AstroDeepak.Domain.Entities;

namespace AstroDeepak.Domain.Abstractions
{
    public interface IPermanentRemedyRepository
    {
        Task<List<PermanentRemedy>> GetByPersonIdAsync(int personId);
        Task<List<PermanentRemedy>> GetByPersonAndNavgrahAsync(int personId, int navgrahId);
        Task AddAsync(int personId, int navgrahId, string remedyName);
        Task RemoveAsync(int personId, int navgrahId, string remedyName);
    }
}