using AstroDeepak.Domain.Entities;

namespace AstroDeepak.Domain.Abstractions
{
    public interface IUsersRemedyRepository
    {
        Task<UserRemedy?> GetByPersonAndNavgrahAsync(int personId, int navgrahId);
        Task<List<UserRemedy>> GetByPersonIdAsync(int personId);
        Task<int> SaveAsync(UserRemedy remedy);
        Task<int> DeleteAsync(int id);
    }
}