using AstroDeepak.Application.DTOs;

namespace AstroDeepak.Application.Interfaces
{
    public interface IPdfExportService
    {
        Task<string> GenerateRemedyReviewPdfAsync(UserRemedyStagingDto staging);
    }
}