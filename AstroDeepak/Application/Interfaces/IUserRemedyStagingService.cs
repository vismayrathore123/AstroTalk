using AstroDeepak.Application.DTOs;

namespace AstroDeepak.Application.Interfaces
{
    public interface IUserRemedyStagingService
    {
        Task<int> SaveAsync(UserRemedyStagingDto dto);
        Task<UserRemedyStagingDto?> GetByIdAsync(int id);
        Task DeleteAsync(int id);
    }
}