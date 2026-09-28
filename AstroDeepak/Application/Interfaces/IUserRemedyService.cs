using AstroDeepak.Application.DTOs;

namespace AstroDeepak.Application.Interfaces
{
    public interface IUserRemedyService
    {
        Task<UserRemedyDto?> GetAsync(int personId, int navgrahId);
        Task SaveSelectedRemediesAsync(int personId, int navgrahId, List<string> selectedRemedyNames);
        Task MarkWhatsAppStatusAsync(int personId, int navgrahId, bool sent);
    }
}