using AstroDeepak.Domain.Entities;

namespace AstroDeepak.Domain.Abstractions
{
    public interface IPrecautionRepository
    {
        Task<List<PrecautionMaster>> GetAllAsync();
        Task AddPrecautionAsync(string text);
        Task UpdatePrecautionAsync(int id, string text);
        Task DeletePrecautionAsync(int id);
    }
}