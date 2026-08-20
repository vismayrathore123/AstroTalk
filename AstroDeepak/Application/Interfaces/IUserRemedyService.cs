using AstroDeepak.Application.DTOs;

namespace AstroDeepak.Application.Interfaces
{
    public interface IUserRemedyService
    {
        Task<UserRemedyDto?> GetAsync(int personId, int navgrahId);

        // Appends selectedRemedyNames to the existing history for this
        // (personId, navgrahId) pair and updates CurrentSuggestedRemedy.
        Task SaveSelectedRemediesAsync(int personId, int navgrahId, List<string> selectedRemedyNames);
    }
}