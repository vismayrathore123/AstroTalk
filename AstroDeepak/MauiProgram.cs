using Microsoft.Extensions.Logging;
using AstroDeepak.Application.Services;
using AstroDeepak.Application.Interfaces;
using AstroDeepak.Infrastructure.Persistence;
using AstroDeepak.Infrastructure.Repositories;
using AstroDeepak.Domain.Abstractions;
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

            builder.Services.AddSingleton<SqliteDbContext>();
            builder.Services.AddSingleton<IPersonRepository, PersonRepository>();
            builder.Services.AddSingleton<IPersonService, PersonService>();
            builder.Services.AddSingleton<IMasterDataRepository, MasterDataRepository>();
            builder.Services.AddSingleton<IRemedyRepository, RemedyRepository>();
            builder.Services.AddSingleton<IUsersRemedyRepository, UsersRemedyRepository>();
            builder.Services.AddSingleton<IUserRemedyService, UserRemedyService>();

            builder.Services.AddTransient<SearchPage>();
            builder.Services.AddTransient<PersonFormPage>();
            builder.Services.AddTransient<NavgrahListPage>();
            builder.Services.AddTransient<RemedySelectionPage>();
            builder.Services.AddTransient<MasterRemedyPage>();
            return builder.Build();
        }
    }
}