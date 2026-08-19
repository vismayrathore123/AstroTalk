using AstroDeepak.Application.DTOs;

namespace AstroDeepak.Application.Interfaces
{
    public interface IPersonService
    {
        Task<List<PersonDto>> GetAllAsync();
        Task<PersonDto?> GetByIdAsync(int id);
        Task<int> SaveAsync(PersonDto dto);
        Task<int> DeleteAsync(int id);

        Task<List<PersonDto>> SearchAsync(string term);
        Task<List<PersonDto>> GetRecentAsync(int count = 10);
    }
}
