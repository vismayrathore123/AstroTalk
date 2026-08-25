using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using AstroDeepak.Application.Services;
using AstroDeepak.Application.Interfaces;
using AstroDeepak.Infrastructure.Persistence;
using AstroDeepak.Infrastructure.Repositories;
using AstroDeepak.Infrastructure.Logging;
using AstroDeepak.Infrastructure.Platform;
using AstroDeepak.Domain.Abstractions;
using AstroDeepak.Views;
using QuestPDF.Infrastructure;

namespace AstroDeepak
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            //QuestPDF.Settings.License = LicenseType.Community; // required once at startup

            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // ---- Cross-cutting infrastructure ----
            builder.Services.AddSingleton<IAppLogger, FileAppLogger>();
            builder.Services.AddSingleton<IDownloadsPathProvider, DownloadsPathProvider>();

            // ---- Data layer ----
            builder.Services.AddSingleton<SqliteDbContext>();
            builder.Services.AddSingleton<IPersonRepository, PersonRepository>();
            builder.Services.AddSingleton<IPersonService, PersonService>();
            builder.Services.AddSingleton<IMasterDataRepository, MasterDataRepository>();
            builder.Services.AddSingleton<IRemedyRepository, RemedyRepository>();
            builder.Services.AddSingleton<IUsersRemedyRepository, UsersRemedyRepository>();
            builder.Services.AddSingleton<IUserRemedyService, UserRemedyService>();
            builder.Services.AddSingleton<IUserRemedyStagingRepository, UserRemedyStagingRepository>();
            builder.Services.AddSingleton<IUserRemedyStagingService, UserRemedyStagingService>();
            builder.Services.AddSingleton<IPdfExportService, PdfExportService>();
            builder.Services.AddSingleton<IPrecautionRepository, PrecautionRepository>();
            builder.Services.AddSingleton<IPermanentRemedyRepository, PermanentRemedyRepository>();

            // ---- Pages ----
            builder.Services.AddTransient<SearchPage>();
            builder.Services.AddTransient<PersonFormPage>();
            builder.Services.AddTransient<NavgrahListPage>();
            builder.Services.AddTransient<PreviewPage>();
            builder.Services.AddTransient<GrahRemedyPage>();
            builder.Services.AddTransient<PrecautionsPage>();

            var app = builder.Build();

            // Catch-all logging: anything that slips past try/catch in the app still
            // lands in the same daily log file instead of disappearing silently.
            var logger = app.Services.GetRequiredService<IAppLogger>();

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                logger.LogError("Unhandled AppDomain exception", e.ExceptionObject as Exception);

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                logger.LogError("Unobserved task exception", e.Exception);
                e.SetObserved();
            };

            logger.LogInfo("App started.");

            return app;
        }
    }
}