using AstroDeepak.Domain.Entities;

namespace AstroDeepak.Domain.Abstractions
{
    public interface IPersonRepository
    {
        Task<List<Person>> GetAllAsync();
        Task<Person?> GetByIdAsync(int id);
        Task<int> SaveAsync(Person person);
        Task<int> DeleteAsync(Person person);
        Task<List<Person>> SearchAsync(string term);
        Task<List<Person>> GetRecentAsync(int count);
    }
}
