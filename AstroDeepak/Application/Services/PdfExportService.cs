using AstroDeepak.Application.DTOs;
using AstroDeepak.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Colors = QuestPDF.Helpers.Colors;

namespace AstroDeepak.Application.Services
{
    public class PdfExportService : IPdfExportService
    {
        private readonly IDownloadsPathProvider _downloadsPathProvider;
        private readonly IAppLogger _logger;

        public PdfExportService(IDownloadsPathProvider downloadsPathProvider, IAppLogger logger)
        {
            _downloadsPathProvider = downloadsPathProvider;
            _logger = logger;
        }

        public Task<string> GenerateRemedyReviewPdfAsync(UserRemedyStagingDto staging)
        {
            var fileName = BuildFileName(staging);
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            try
            {
                BuildDocument(staging).GeneratePdf(filePath);
                _logger.LogInfo($"PDF generated in cache for share/WhatsApp flow. Path={filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed generating cache PDF for PersonId={staging.PersonId}", ex);
                throw;
            }

            return Task.FromResult(filePath);
        }

        public async Task<string> SaveRemedyReviewPdfToDownloadsAsync(UserRemedyStagingDto staging)
        {
            var downloadsFolder = await _downloadsPathProvider.GetDownloadsFolderAsync();
            var fileName = BuildFileName(staging);
            var filePath = Path.Combine(downloadsFolder, fileName);

            try
            {
                BuildDocument(staging).GeneratePdf(filePath);
                _logger.LogInfo($"PDF saved directly to Downloads (no dialog). Path={filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed saving PDF to downloads folder '{downloadsFolder}' for PersonId={staging.PersonId}", ex);
                throw;
            }

            return filePath;
        }

        private static string BuildFileName(UserRemedyStagingDto staging)
    => $"Kundli_{staging.Name?.Replace(" ", "_")}_{DateTime.Now:yyyyMMddHHmmss}.pdf";

        private static IDocument BuildDocument(UserRemedyStagingDto staging)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Text("AstroDeepak - Kundli Remedy Report")
                        .FontSize(18)
                        .Bold()
                        .FontColor(Colors.Orange.Darken2);

                    page.Content()
                        .PaddingTop(15)
                        .Column(col =>
                        {
                            // Person Details
                            col.Item().Text($"Name: {staging.Name}").Bold();

                            col.Item().Text(
                                $"Father's Name: {(string.IsNullOrWhiteSpace(staging.FatherName)
                                    ? "-"
                                    : staging.FatherName)}");

                            col.Item().Text(
                                $"Date of Birth: {staging.DOB:dd MMM yyyy}");

                            if (!string.IsNullOrWhiteSpace(staging.Time))
                            {
                                col.Item().Text(
                                    $"Time of Birth: {staging.Time}");
                            }

                            if (!string.IsNullOrWhiteSpace(staging.BirthPlace))
                            {
                                col.Item().Text(
                                    $"Birth Place: {staging.BirthPlace}");
                            }


                            // ============================
                            // REMEDIES - SHOW FIRST
                            // ============================
                            foreach (var selection in staging.Selections)
                            {
                                col.Item()
                                    .PaddingTop(15)
                                    .Text($"Remedies for {selection.NavgrahName}")
                                    .FontSize(14)
                                    .Bold()
                                    .FontColor(Colors.Orange.Darken1);

                                foreach (var remedy in selection.Remedies)
                                {
                                    var tags = new List<string>();

                                    if (remedy.IsPermanent)
                                        tags.Add("Permanent");

                                    if (remedy.IsYearly)
                                        tags.Add("Yearly");

                                    var suffix = tags.Count > 0
                                        ? $"  ({string.Join(", ", tags)})"
                                        : string.Empty;

                                    col.Item()
                                        .PaddingLeft(10)
                                        .Text($"• {remedy.Name}{suffix}");
                                }
                            }


                            // ============================
                            // PRECAUTIONS - SHOW AFTER ALL GRAH REMEDIES
                            // ============================
                            if (staging.SelectedPrecautions != null &&
                                staging.SelectedPrecautions.Any())
                            {
                                col.Item()
                                    .PaddingTop(20)
                                    .Text("Precautions")
                                    .FontSize(14)
                                    .Bold()
                                    .FontColor(Colors.Orange.Darken1);

                                foreach (var precaution in staging.SelectedPrecautions)
                                {
                                    col.Item()
                                        .PaddingLeft(10)
                                        .Text($"• {precaution}");
                                }
                            }
                        });


                    // Footer
                    page.Footer()
                        .AlignCenter()
                        .Text(t =>
                        {
                            t.Span("Generated on ").FontSize(9);

                            t.Span(
                                DateTime.Now.ToString("dd MMM yyyy, hh:mm tt")
                            ).FontSize(9);
                        });
                });
            });
        }
    }
}