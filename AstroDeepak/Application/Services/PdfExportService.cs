using AstroDeepak.Application.DTOs;
using AstroDeepak.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;
using Colors = QuestPDF.Helpers.Colors;

namespace AstroDeepak.Application.Services
{
    public class PdfExportService : IPdfExportService
    {
        private readonly IDownloadsPathProvider _downloadsPathProvider;
        private readonly IAppLogger _logger;
        private static byte[]? _rawWatermarkBytes;
        private static byte[]? _processedWatermarkBytes;
       private static byte[]? _logoBytes;

        private static readonly object _watermarkLock = new();
        private const float WatermarkOpacity = 0.05f;

        private const bool ClipWatermarkToCircle = false;
        public PdfExportService(
            IDownloadsPathProvider downloadsPathProvider,
            IAppLogger logger)
        {
            _downloadsPathProvider = downloadsPathProvider;
            _logger = logger;
        }

        public Task<string> GenerateRemedyReviewPdfAsync(
            UserRemedyStagingDto staging)
        {
            var fileName = BuildFileName(staging);

            var filePath = System.IO.Path.Combine(
                FileSystem.CacheDirectory,
                fileName);

            try
            {
                BuildDocument(staging).GeneratePdf(filePath);

                _logger.LogInfo(
                    $"PDF generated in cache for share/WhatsApp flow. Path={filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"Failed generating cache PDF for PersonId={staging.PersonId}",
                    ex);

                throw;
            }

            return Task.FromResult(filePath);
        }

        public async Task<string> SaveRemedyReviewPdfToDownloadsAsync(
            UserRemedyStagingDto staging)
        {
            var downloadsFolder =
                await _downloadsPathProvider.GetDownloadsFolderAsync();

            var fileName = BuildFileName(staging);

            var filePath = System.IO.Path.Combine(
                downloadsFolder,
                fileName);

            try
            {
                BuildDocument(staging).GeneratePdf(filePath);

                _logger.LogInfo(
                    $"PDF saved directly to Downloads (no dialog). Path={filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"Failed saving PDF to downloads folder '{downloadsFolder}' for PersonId={staging.PersonId}",
                    ex);

                throw;
            }

            return filePath;
        }



        private static string BuildFileName(
            UserRemedyStagingDto staging)
        {
            var safeName = string.Join(
                "_",
                (staging.Name ?? "Unknown")
                    .Split(
                        System.IO.Path.GetInvalidFileNameChars(),
                        StringSplitOptions.RemoveEmptyEntries));

            return $"Kundli_{safeName}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
        }
        private byte[] LoadRawWatermarkBytes()
        {
            lock (_watermarkLock)
            {
                if (_rawWatermarkBytes != null)
                    return _rawWatermarkBytes;

                try
                {
                    var assembly =
                        typeof(PdfExportService).Assembly;

                    const string resourceName =
                        "AstroDeepak.Application.Assets.horoscope.png";

                    using var stream =
                        assembly.GetManifestResourceStream(resourceName);

                    if (stream == null)
                    {
                        _logger.LogWarning(
                            $"[WATERMARK] NOT FOUND: {resourceName}");

                        _rawWatermarkBytes =
                            Array.Empty<byte>();

                        return _rawWatermarkBytes;
                    }

                    using var ms = new MemoryStream();

                    stream.CopyTo(ms);

                    _logger.LogInfo(
                        $"[WATERMARK] Loaded OK, {ms.Length} bytes");

                    _rawWatermarkBytes = ms.ToArray();

                    return _rawWatermarkBytes;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        "[WATERMARK] Failed to load watermark image",
                        ex);

                    _rawWatermarkBytes =
                        Array.Empty<byte>();

                    return _rawWatermarkBytes;
                }
            }
        }

        private byte[] LoadLogoBytes()
        {
            lock (_watermarkLock)
            {
                if (_logoBytes != null)
                    return _logoBytes;

                try
                {
                    var assembly = typeof(PdfExportService).Assembly;

                    const string resourceName =
                        "AstroDeepak.Application.Assets.images.jpg";

                    _logger.LogInfo(
                        $"[LOGO] Looking for resource: {resourceName}");

                    using var stream =
                        assembly.GetManifestResourceStream(resourceName);

                    if (stream == null)
                    {
                        _logger.LogWarning(
                            $"[LOGO] NOT FOUND: {resourceName}");

                     
                        foreach (var name in assembly.GetManifestResourceNames())
                        {
                            _logger.LogInfo(
                                $"[LOGO] Available resource: {name}");
                        }

                        _logoBytes = Array.Empty<byte>();
                        return _logoBytes;
                    }

                    using var ms = new MemoryStream();

                    stream.CopyTo(ms);

                    _logoBytes = ms.ToArray();

                    _logger.LogInfo(
                        $"[LOGO] Loaded successfully: {_logoBytes.Length} bytes");

                    return _logoBytes;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        "[LOGO] Failed to load images.jpg",
                        ex);

                    _logoBytes = Array.Empty<byte>();

                    return _logoBytes;
                }
            }
        }

        private byte[] GetProcessedWatermarkBytes()
        {
            lock (_watermarkLock)
            {
                if (_processedWatermarkBytes != null)
                    return _processedWatermarkBytes;

                var raw = LoadRawWatermarkBytes();

                if (raw.Length == 0)
                {
                    _processedWatermarkBytes =
                        Array.Empty<byte>();

                    return _processedWatermarkBytes;
                }

                try
                {
                    using var original =
                        SKBitmap.Decode(raw);

                    if (original == null)
                    {
                        _logger.LogWarning(
                            "[WATERMARK] Could not decode raw watermark bytes as an image.");

                        _processedWatermarkBytes =
                            Array.Empty<byte>();

                        return _processedWatermarkBytes;
                    }
                    var size =
                        Math.Min(
                            original.Width,
                            original.Height);

                    var info = new SKImageInfo(
                        size,
                        size,
                        SKColorType.Rgba8888,
                        SKAlphaType.Premul);

                    using var sourceBitmap =
                        new SKBitmap(info);

                    using var sourceCanvas =
                        new SKCanvas(sourceBitmap);

                    sourceCanvas.Clear(
                        SKColors.Transparent);

                    var srcX =
                        (original.Width - size) / 2f;

                    var srcY =
                        (original.Height - size) / 2f;

                    var srcRect =
                        new SKRect(
                            srcX,
                            srcY,
                            srcX + size,
                            srcY + size);

                    var destRect =
                        new SKRect(
                            0,
                            0,
                            size,
                            size);


                    using var sourcePaint =
                        new SKPaint
                        {
                            IsAntialias = true
                        };


                    sourceCanvas.DrawBitmap(
                        original,
                        srcRect,
                        destRect,
                        sourcePaint);

                    using var finalBitmap =
                        new SKBitmap(info);

                    using var finalCanvas =
                        new SKCanvas(finalBitmap);

                    finalCanvas.Clear(
                        SKColors.Transparent);

                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            var pixel =
                                sourceBitmap.GetPixel(
                                    x,
                                    y);

                            byte r = pixel.Red;
                            byte g = pixel.Green;
                            byte b = pixel.Blue;
                            byte a = pixel.Alpha;

                            if (a == 0)
                                continue;


                           
                            int min =
                                Math.Min(
                                    r,
                                    Math.Min(g, b));

                            int max =
                                Math.Max(
                                    r,
                                    Math.Max(g, b));


                            bool isWhiteBackground =
                                min >= 238 &&
                                (max - min) <= 18;


                            if (isWhiteBackground)
                            {
                                finalBitmap.SetPixel(
                                    x,
                                    y,
                                    SKColors.Transparent);

                                continue;
                            }


                            byte finalAlpha =
                                (byte)(
                                    a * WatermarkOpacity);


                            finalBitmap.SetPixel(
                                x,
                                y,
                                new SKColor(
                                    r,
                                    g,
                                    b,
                                    finalAlpha));
                        }
                    }
                    if (ClipWatermarkToCircle)
                    {
                        var circleInfo =
                            new SKImageInfo(
                                size,
                                size,
                                SKColorType.Rgba8888,
                                SKAlphaType.Premul);

                        using var circleSurface =
                            SKSurface.Create(circleInfo);

                        var circleCanvas =
                            circleSurface.Canvas;

                        circleCanvas.Clear(
                            SKColors.Transparent);

                        var radius =
                            size / 2f;

                        using var clipPath =
                            new SKPath();

                        clipPath.AddCircle(
                            radius,
                            radius,
                            radius);

                        circleCanvas.ClipPath(
                            clipPath,
                            antialias: true);

                        using var circlePaint =
                            new SKPaint
                            {
                                IsAntialias = true
                            };

                        circleCanvas.DrawBitmap(
                            finalBitmap,
                            0,
                            0,
                            circlePaint);

                        using var circleImage =
                            circleSurface.Snapshot();

                        using var circleData =
                            circleImage.Encode(
                                SKEncodedImageFormat.Png,
                                100);

                        _processedWatermarkBytes =
                            circleData.ToArray();
                    }
                    else
                    {
                        using var image =
                            SKImage.FromBitmap(
                                finalBitmap);

                        using var data =
                            image.Encode(
                                SKEncodedImageFormat.Png,
                                100);

                        _processedWatermarkBytes =
                            data.ToArray();
                    }


                    _logger.LogInfo(
                        $"[WATERMARK] Processed OK ({WatermarkOpacity:P0} opacity), " +
                        $"{_processedWatermarkBytes.Length} bytes");

                    return _processedWatermarkBytes;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        "[WATERMARK] Failed to process watermark image",
                        ex);

                    _processedWatermarkBytes =
                        Array.Empty<byte>();

                    return _processedWatermarkBytes;
                }
            }
        }
        private IDocument BuildDocument(
            UserRemedyStagingDto staging)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    

                    page.Size(PageSizes.A4);

                    page.Margin(30);

                    page.DefaultTextStyle(
                        x => x
                            .FontSize(11)
                            .FontColor(
                                Colors.Grey.Darken3));

                    var watermarkBytes =
                        GetProcessedWatermarkBytes();

                    if (watermarkBytes.Length > 0)
                    {
                        page.Background()
                            .AlignCenter()
                            .AlignMiddle()
                            .Width(380)
                            .Image(watermarkBytes)
                            .FitWidth();
                    }
                    page.Header().Column(header =>
                    {
                        header.Item().Row(row =>
                        {
                          
                            row.RelativeItem().Column(c =>
                            {
                                c.Item()
                                    .Text("Deepak Jaiswal")
                                    .FontSize(22)
                                    .Bold()
                                    .FontColor(
                                        Colors.Orange.Darken3);

                                c.Item()
                                    .Text("Kundli Remedy Report")
                                    .FontSize(11)
                                    .FontColor(
                                        Colors.Grey.Darken1);
                            });

                            row.ConstantItem(100)
                                .AlignRight()
                                .AlignBottom()
                                .Column(right =>
                                {
                                    var logoBytes =
                                        LoadLogoBytes();

                                    if (logoBytes.Length > 0)
                                    {
                                        right.Item()
                                            .AlignRight()
                                            .Width(48)
                                            .Height(48)
                                            .Image(logoBytes)
                                            .FitArea();
                                    }

                                    right.Item()
                                        .PaddingTop(3)
                                        .Text(
                                            DateTime.Now.ToString(
                                                "dd MMM yyyy"))
                                        .FontSize(9)
                                        .FontColor(
                                            Colors.Grey.Medium);
                                });
                        });
                        header.Item()
                            .PaddingTop(10)
                            .LineHorizontal(1.5f)
                            .LineColor(
                                Colors.Orange.Lighten2);
                    });
                    page.Content()
                        .PaddingTop(18)
                        .Column(col =>
                        {
                            col.Spacing(16);

                            col.Item()
                                .Background(
                                    Colors.Grey.Lighten5)
                                .Border(1)
                                .BorderColor(
                                    Colors.Grey.Lighten2)
                                .CornerRadius(6)
                                .Padding(14)
                                .Column(person =>
                                {
                                    person.Spacing(6);


                                    // Person name
                                    person.Item()
                                    .Text(staging.Name)
                                    .FontSize(16)
                                    .Bold()
                                    .FontColor(
                                        Colors.Black);

                                    person.Item().Row(r =>
                                    {
                                        r.RelativeItem().Text(t =>
                                        {
                                            t.DefaultTextStyle(
                                            x => x.FontSize(
                                                10.5f));

                                            t.Span(
                                            "Father's Name: ")
                                            .SemiBold()
                                            .FontColor(
                                                Colors.Grey.Darken2);

                                            t.Span(
                                            string.IsNullOrWhiteSpace(
                                                staging.FatherName)
                                                ? "-"
                                                : staging.FatherName)
                                            .FontColor(
                                                Colors.Grey.Darken1);
                                        });


                                        r.RelativeItem().Text(t =>
                                        {
                                            t.DefaultTextStyle(
                                            x => x.FontSize(
                                                10.5f));

                                            t.Span(
                                            "Date of Birth: ")
                                            .SemiBold()
                                            .FontColor(
                                                Colors.Grey.Darken2);

                                            t.Span(
                                            $"{staging.DOB:dd MMM yyyy}")
                                            .FontColor(
                                                Colors.Grey.Darken1);
                                        });
                                    });
                                    if (
                                    !string.IsNullOrWhiteSpace(
                                        staging.Time)
                                    ||
                                    !string.IsNullOrWhiteSpace(
                                        staging.BirthPlace))
                                    {
                                        person.Item().Row(r =>
                                        {
                                            r.RelativeItem().Text(t =>
                                            {
                                                t.DefaultTextStyle(
                                                x => x.FontSize(
                                                    10.5f));

                                                t.Span(
                                                "Time of Birth: ")
                                                .SemiBold()
                                                .FontColor(
                                                    Colors.Grey.Darken2);

                                                t.Span(
                                                string.IsNullOrWhiteSpace(
                                                    staging.Time)
                                                    ? "-"
                                                    : staging.Time)
                                                .FontColor(
                                                    Colors.Grey.Darken1);
                                            });


                                            r.RelativeItem().Text(t =>
                                            {
                                                t.DefaultTextStyle(
                                                x => x.FontSize(
                                                    10.5f));

                                                t.Span(
                                                "Birth Place: ")
                                                .SemiBold()
                                                .FontColor(
                                                    Colors.Grey.Darken2);

                                                t.Span(
                                                string.IsNullOrWhiteSpace(
                                                    staging.BirthPlace)
                                                    ? "-"
                                                    : staging.BirthPlace)
                                                .FontColor(
                                                    Colors.Grey.Darken1);
                                            });
                                        });
                                    }
                                });
                            foreach (
                                var selection
                                in staging.Selections)
                            {
                                if (
                                    selection.Remedies.Count == 0)
                                    continue;


                                col.Item().Column(sec =>
                                {
                                    sec.Spacing(6);

                                    sec.Item().Row(r =>
                                    {
                                        r.AutoItem()
                                            .Width(4)
                                            .Height(16)
                                            .Background(
                                                Colors.Orange.Darken1);


                                        r.RelativeItem()
                                            .PaddingLeft(8)
                                            .AlignMiddle()
                                            .Text(
                                                $"Remedies for {selection.NavgrahName}")
                                            .FontSize(13)
                                            .Bold()
                                            .FontColor(
                                                Colors.Orange.Darken2);
                                    });

                                    foreach (
                                        var remedy
                                        in selection.Remedies)
                                    {
                                        sec.Item()
                                            .Border(1)
                                            .BorderColor(
                                                Colors.Grey.Lighten3)
                                            .CornerRadius(4)
                                            .Padding(10)
                                            .Row(rr =>
                                            {
                                                rr.AutoItem()
                                                .AlignMiddle()
                                                .Width(6)
                                                .Height(6)
                                                .Background(
                                                    Colors.Orange.Medium);


                                                rr.RelativeItem()
                                                .PaddingLeft(10)
                                                .AlignMiddle()
                                                .Text(
                                                    remedy.Name)
                                                .FontSize(11.5f)
                                                .FontColor(
                                                    Colors.Grey.Darken4);


                                                rr.AutoItem()
                                                .Row(tags =>
                                                {
                                                    // Yearly
                                                    if (remedy.IsYearly)
                                                    {
                                                        tags.AutoItem()
                                                        .PaddingLeft(4)
                                                        .Background(
                                                            Colors.Blue.Lighten4)
                                                        .CornerRadius(3)
                                                        .Padding(4)
                                                        .Text("Yearly")
                                                        .FontSize(8)
                                                        .FontColor(
                                                            Colors.Blue.Darken2);
                                                    }


                                                    // Permanent
                                                    if (remedy.IsPermanent)
                                                    {
                                                        tags.AutoItem()
                                                        .PaddingLeft(4)
                                                        .Background(
                                                            Colors.Orange.Lighten4)
                                                        .CornerRadius(3)
                                                        .Padding(4)
                                                        .Text("Permanent")
                                                        .FontSize(8)
                                                        .FontColor(
                                                            Colors.Orange.Darken2);
                                                    }
                                                });
                                            });
                                    }
                                });
                            }

                            if (
                                staging.SelectedPrecautions != null
                                &&
                                staging.SelectedPrecautions.Any())
                            {
                                col.Item().Column(sec =>
                                {
                                    sec.Spacing(6);

                                    sec.Item().Row(r =>
                                    {
                                        r.AutoItem()
                                            .Width(4)
                                            .Height(16)
                                            .Background(
                                                Colors.Orange.Darken1);


                                        r.RelativeItem()
                                            .PaddingLeft(8)
                                            .AlignMiddle()
                                            .Text("Precautions")
                                            .FontSize(13)
                                            .Bold()
                                            .FontColor(
                                                Colors.Orange.Darken2);
                                    });

                                    foreach (
                                        var precaution
                                        in staging.SelectedPrecautions)
                                    {
                                        sec.Item().Row(rr =>
                                        {
                                            rr.AutoItem()
                                                .PaddingRight(8)
                                                .Text("•")
                                                .FontSize(12)
                                                .FontColor(
                                                    Colors.Orange.Medium);


                                            rr.RelativeItem()
                                                .Text(precaution)
                                                .FontSize(11)
                                                .FontColor(
                                                    Colors.Grey.Darken2);
                                        });
                                    }
                                });
                            }
                        });

                    page.Footer().Column(footer =>
                    {
                        footer.Item()
                            .PaddingBottom(6)
                            .LineHorizontal(0.75f)
                            .LineColor(
                                Colors.Grey.Lighten2);


                        footer.Item().Row(row =>
                        {

                            row.RelativeItem().Text(t =>
                            {
                                t.Span("Generated on ")
                                    .FontSize(9)
                                    .FontColor(
                                        Colors.Grey.Medium);

                                t.Span(
                                    DateTime.Now.ToString(
                                        "dd MMM yyyy, hh:mm tt"))
                                    .FontSize(9)
                                    .FontColor(
                                        Colors.Grey.Medium);
                            });


                            row.RelativeItem()
                                .AlignRight()
                                .Text(t =>
                                {
                                    t.Span("Page ")
                                        .FontSize(9)
                                        .FontColor(
                                            Colors.Grey.Medium);

                                    t.CurrentPageNumber()
                                        .FontSize(9)
                                        .FontColor(
                                            Colors.Grey.Medium);

                                    t.Span(" of ")
                                        .FontSize(9)
                                        .FontColor(
                                            Colors.Grey.Medium);

                                    t.TotalPages()
                                        .FontSize(9)
                                        .FontColor(
                                            Colors.Grey.Medium);
                                });
                        });
                    });
                });
            });
        }
    }
}