using AstroDeepak.Application.DTOs;

namespace AstroDeepak.Application.Interfaces
{
    public interface IPdfExportService
    {
        /// <summary>
        /// Generates the PDF into the app's cache folder. Used only when we need a
        /// file to hand off to the OS share sheet (the WhatsApp flow needs this,
        /// since the share sheet reads from a real file path).
        /// </summary>
        Task<string> GenerateRemedyReviewPdfAsync(UserRemedyStagingDto staging);

        /// <summary>
        /// Generates the PDF and writes it straight into the platform's Downloads
        /// folder (see IDownloadsPathProvider) - no share dialog, no user interaction.
        /// This is what "Confirm" (without WhatsApp) uses.
        /// </summary>
        Task<string> SaveRemedyReviewPdfToDownloadsAsync(UserRemedyStagingDto staging);
    }
}