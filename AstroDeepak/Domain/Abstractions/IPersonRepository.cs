using AstroDeepak.Domain.Entities;

namespace AstroDeepak.Domain.Abstractions
{
    public interface IPersonRepository
    {
        Task<List<Person>> GetAllAsync();
        Task<Person?> GetByIdAsync(int id);
        Task<int> SaveAsync(Person person);
        Task<int> DeleteAsync(Person person);

        // NEW: used by the "New Kundli" search box
        Task<List<Person>> SearchAsync(string term);

        // NEW: used by the "recent added" list under the search box
        Task<List<Person>> GetRecentAsync(int count);
    }
}
