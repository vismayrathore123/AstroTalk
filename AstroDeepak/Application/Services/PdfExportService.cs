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
        {
            var safeName = string.Join("_", (staging.Name ?? "Unknown")
                .Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
            return $"Kundli_{safeName}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
        }


        private static IDocument BuildDocument(UserRemedyStagingDto staging)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(11).FontColor(Colors.Grey.Darken3));

                    // ============================
                    // HEADER
                    // ============================
                    page.Header().Column(header =>
                    {
                        header.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Deepak Jaiswal").FontSize(22).Bold().FontColor(Colors.Orange.Darken3);
                                c.Item().Text("Kundli Remedy Report").FontSize(11).FontColor(Colors.Grey.Darken1);
                            });

                            row.ConstantItem(100).AlignRight().AlignBottom()
                                .Text(DateTime.Now.ToString("dd MMM yyyy"))
                                .FontSize(9).FontColor(Colors.Grey.Medium);
                        });

                        header.Item().PaddingTop(10).LineHorizontal(1.5f).LineColor(Colors.Orange.Lighten2);
                    });

                    // ============================
                    // CONTENT
                    // ============================
                    page.Content().PaddingTop(18).Column(col =>
                    {
                        col.Spacing(16);

                        // ---- Person details card ----
                        col.Item()
                            .Background(Colors.Grey.Lighten5)
                            .Border(1).BorderColor(Colors.Grey.Lighten2)
                            .CornerRadius(6)
                            .Padding(14)
                            .Column(person =>
                            {
                                person.Spacing(6);

                                person.Item().Text(staging.Name).FontSize(16).Bold().FontColor(Colors.Black);

                                person.Item().Row(r =>
                                {
                                    r.RelativeItem().Text(t =>
                                    {
                                        t.DefaultTextStyle(x => x.FontSize(10.5f));
                                        t.Span("Father's Name: ").SemiBold().FontColor(Colors.Grey.Darken2);
                                        t.Span(string.IsNullOrWhiteSpace(staging.FatherName) ? "-" : staging.FatherName)
                                            .FontColor(Colors.Grey.Darken1);
                                    });

                                    r.RelativeItem().Text(t =>
                                    {
                                        t.DefaultTextStyle(x => x.FontSize(10.5f));
                                        t.Span("Date of Birth: ").SemiBold().FontColor(Colors.Grey.Darken2);
                                        t.Span($"{staging.DOB:dd MMM yyyy}").FontColor(Colors.Grey.Darken1);
                                    });
                                });

                                if (!string.IsNullOrWhiteSpace(staging.Time) || !string.IsNullOrWhiteSpace(staging.BirthPlace))
                                {
                                    person.Item().Row(r =>
                                    {
                                        r.RelativeItem().Text(t =>
                                        {
                                            t.DefaultTextStyle(x => x.FontSize(10.5f));
                                            t.Span("Time of Birth: ").SemiBold().FontColor(Colors.Grey.Darken2);
                                            t.Span(string.IsNullOrWhiteSpace(staging.Time) ? "-" : staging.Time)
                                                .FontColor(Colors.Grey.Darken1);
                                        });

                                        r.RelativeItem().Text(t =>
                                        {
                                            t.DefaultTextStyle(x => x.FontSize(10.5f));
                                            t.Span("Birth Place: ").SemiBold().FontColor(Colors.Grey.Darken2);
                                            t.Span(string.IsNullOrWhiteSpace(staging.BirthPlace) ? "-" : staging.BirthPlace)
                                                .FontColor(Colors.Grey.Darken1);
                                        });
                                    });
                                }
                            });

                        // ---- Remedies, grouped by Grah ----
                        foreach (var selection in staging.Selections)
                        {
                            if (selection.Remedies.Count == 0) continue;

                            col.Item().Column(sec =>
                            {
                                sec.Spacing(6);

                                sec.Item().Row(r =>
                                {
                                    r.AutoItem().Width(4).Height(16).Background(Colors.Orange.Darken1);
                                    r.RelativeItem().PaddingLeft(8).AlignMiddle()
                                        .Text($"Remedies for {selection.NavgrahName}")
                                        .FontSize(13).Bold().FontColor(Colors.Orange.Darken2);
                                });

                                foreach (var remedy in selection.Remedies)
                                {
                                    sec.Item()
                                        .Border(1).BorderColor(Colors.Grey.Lighten3)
                                        .CornerRadius(4)
                                        .Padding(10)
                                        .Row(rr =>
                                        {
                                            rr.AutoItem().AlignMiddle().Width(6).Height(6)
                                                .Background(Colors.Orange.Medium);

                                            rr.RelativeItem().PaddingLeft(10).AlignMiddle()
                                                .Text(remedy.Name).FontSize(11.5f).FontColor(Colors.Grey.Darken4);

                                            rr.AutoItem().Row(tags =>
                                            {
                                                if (remedy.IsYearly)
                                                {
                                                    tags.AutoItem().PaddingLeft(4)
                                                        .Background(Colors.Blue.Lighten4).CornerRadius(3).Padding(4)
                                                        .Text("Yearly").FontSize(8).FontColor(Colors.Blue.Darken2);
                                                }
                                                if (remedy.IsPermanent)
                                                {
                                                    tags.AutoItem().PaddingLeft(4)
                                                        .Background(Colors.Orange.Lighten4).CornerRadius(3).Padding(4)
                                                        .Text("Permanent").FontSize(8).FontColor(Colors.Orange.Darken2);
                                                }
                                            });
                                        });
                                }
                            });
                        }

                        // ---- Precautions ----
                        if (staging.SelectedPrecautions != null && staging.SelectedPrecautions.Any())
                        {
                            col.Item().Column(sec =>
                            {
                                sec.Spacing(6);

                                sec.Item().Row(r =>
                                {
                                    r.AutoItem().Width(4).Height(16).Background(Colors.Orange.Darken1);
                                    r.RelativeItem().PaddingLeft(8).AlignMiddle()
                                        .Text("Precautions").FontSize(13).Bold().FontColor(Colors.Orange.Darken2);
                                });

                                foreach (var precaution in staging.SelectedPrecautions)
                                {
                                    sec.Item().Row(rr =>
                                    {
                                        rr.AutoItem().PaddingRight(8)
                                            .Text("•").FontSize(12).FontColor(Colors.Orange.Medium);
                                        rr.RelativeItem()
                                            .Text(precaution).FontSize(11).FontColor(Colors.Grey.Darken2);
                                    });
                                }
                            });
                        }
                    });

                    // ============================
                    // FOOTER
                    // ============================
                    page.Footer().Column(footer =>
                    {
                        footer.Item().PaddingBottom(6).LineHorizontal(0.75f).LineColor(Colors.Grey.Lighten2);

                        footer.Item().Row(row =>
                        {
                            row.RelativeItem().Text(t =>
                            {
                                t.Span("Generated on ").FontSize(9).FontColor(Colors.Grey.Medium);
                                t.Span(DateTime.Now.ToString("dd MMM yyyy, hh:mm tt"))
                                    .FontSize(9).FontColor(Colors.Grey.Medium);
                            });

                            row.RelativeItem().AlignRight().Text(t =>
                            {
                                t.Span("Page ").FontSize(9).FontColor(Colors.Grey.Medium);
                                t.CurrentPageNumber().FontSize(9).FontColor(Colors.Grey.Medium);
                                t.Span(" of ").FontSize(9).FontColor(Colors.Grey.Medium);
                                t.TotalPages().FontSize(9).FontColor(Colors.Grey.Medium);
                            });
                        });
                    });
                });
            });
        }
    }
}