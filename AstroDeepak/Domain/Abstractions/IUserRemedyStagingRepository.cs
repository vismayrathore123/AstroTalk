using AstroDeepak.Domain.Entities;

namespace AstroDeepak.Domain.Abstractions
{
    public interface IUserRemedyStagingRepository
    {
        Task<int> SaveAsync(UserRemedyStaging staging);
        Task<UserRemedyStaging?> GetByIdAsync(int id);
        Task DeleteAsync(int id);
    }
}