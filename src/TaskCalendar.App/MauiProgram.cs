using Microsoft.Extensions.Logging;
using TaskCalendar.App.Services;

namespace TaskCalendar.App;

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
            });

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddSingleton(new ApiOptions());
        builder.Services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<ApiOptions>();
            return new HttpClient
            {
                BaseAddress = new Uri(options.BaseUrl)
            };
        });
        builder.Services.AddSingleton<CalendarApiClient>();
        builder.Services.AddSingleton<SessionService>();
        builder.Services.AddSingleton<AppLocalizer>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
