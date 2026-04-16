using Microsoft.Extensions.Logging;
using SoPorHoje.App.Services;
using SoPorHoje.App.ViewModels;
using SoPorHoje.App.Views;

namespace SoPorHoje.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Services
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<ChipService>();
        builder.Services.AddSingleton<MeetingService>();

        // ViewModels
        builder.Services.AddTransient<OnboardingViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<ChipsViewModel>();
        builder.Services.AddTransient<SOSViewModel>();
        builder.Services.AddTransient<MeetingsViewModel>();
        builder.Services.AddTransient<ProgramViewModel>();
        builder.Services.AddTransient<StepsViewModel>();
        builder.Services.AddTransient<TraditionsViewModel>();
        builder.Services.AddTransient<PromisesViewModel>();
        builder.Services.AddTransient<JustForTodayViewModel>();
        builder.Services.AddTransient<PrayersViewModel>();
        builder.Services.AddTransient<HaltCheckViewModel>();

        // Pages
        builder.Services.AddTransient<OnboardingPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<ChipsPage>();
        builder.Services.AddTransient<SOSPage>();
        builder.Services.AddTransient<MeetingsPage>();
        builder.Services.AddTransient<ProgramPage>();
        builder.Services.AddTransient<StepsPage>();
        builder.Services.AddTransient<TraditionsPage>();
        builder.Services.AddTransient<PromisesPage>();
        builder.Services.AddTransient<JustForTodayPage>();
        builder.Services.AddTransient<PrayersPage>();
        builder.Services.AddTransient<HaltCheckPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
