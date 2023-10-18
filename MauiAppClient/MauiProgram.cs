
using Havit.Blazor.Components.Web;
using MauiAppClient.Authentication;
using MauiAppClient.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Syncfusion.Blazor;

namespace MauiAppClient
{
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
            builder.Services.AddSyncfusionBlazor();
            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddHxServices();
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddSingleton<BaseServices>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
