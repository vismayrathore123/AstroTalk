using Microsoft.Extensions.Logging;
using AstroDeepak.Application.Services;
using AstroDeepak.Infrastructure.Persistence;
using AstroDeepak.Infrastructure.Repositories;
using AstroDeepak.Domain.Abstractions;
using AstroDeepak.Application.Interfaces;
using AstroDeepak.Views;

namespace AstroDeepak
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
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

            // Data layer - one shared sqlite connection for the app's lifetime
            builder.Services.AddSingleton<SqliteDbContext>();
            builder.Services.AddSingleton<IPersonRepository, PersonRepository>();
            builder.Services.AddSingleton<IPersonService, PersonService>();

            // Pages - Transient because Shell creates a fresh instance every
            // time you navigate to the route (e.g. re-opening the form for a
            // different person should not reuse old page state).
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<SearchPage>();
            builder.Services.AddTransient<PersonFormPage>();
            builder.Services.AddTransient<NavgrahListPage>();

            return builder.Build();
        }
    }
}
