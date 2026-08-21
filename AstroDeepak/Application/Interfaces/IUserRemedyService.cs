using AstroDeepak.Application.DTOs;

namespace AstroDeepak.Application.Interfaces
{
    public interface IUserRemedyService
    {
        Task<UserRemedyDto?> GetAsync(int personId, int navgrahId);
        Task SaveSelectedRemediesAsync(int personId, int navgrahId, List<string> selectedRemedyNames);

        // Flips the WhatsApp flag for this person+Grah row after a send attempt.
        Task MarkWhatsAppStatusAsync(int personId, int navgrahId, bool sent);
    }
}