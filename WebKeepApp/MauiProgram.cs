using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using WebKeepApp.Interfaces;
using WebKeepApp.Services;
using System.Threading.Tasks;
using WebKeepApp.Utils;
using WebKeepApp.Pages;

namespace WebKeepApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            try
            {
                var builder = MauiApp.CreateBuilder();
                DLogger.Log("Starting CreateMauiApp");

                builder
                    .UseMauiApp<App>()
                    .ConfigureFonts(fonts =>
                    {
                        fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                        fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    });
                DLogger.Log("MAUI App and fonts configured");

#if DEBUG
                builder.Logging.AddDebug();
                DLogger.Log("Debug logging added");
#endif
                // Load configuration
                try
                {
                    var assembly = typeof(MauiProgram).Assembly;
                    using var stream = assembly.GetManifestResourceStream("WebKeepApp.appsettings.json")
                        ?? throw new InvalidOperationException("Could not load appsettings.json");

                    builder.Configuration.AddJsonStream(stream);
                    DLogger.Log("Configuration loaded successfully");
                }
                catch (Exception ex)
                {
                    DLogger.Log("Failed to load configuration", ex);
                    throw;
                }

                // Register pages as services
                builder.Services.AddTransient<LoginPage>();
                builder.Services.AddTransient<MainPage>();
                builder.Services.AddTransient<CreatePage>();
                DLogger.Log("Pages registered as services");

                // Initialize backup service
                builder.Services.AddTransient<IBackupService, BackupService>();
                DLogger.Log("Backup service registered as service");

                builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
                DLogger.Log("Backup service registered as service");

                // Initialize HTTP client for backup API
                builder.Services.AddHttpClient("BackupClient")
                .ConfigureHttpClient((client, options) =>
                {
                    options.Timeout = TimeSpan.FromSeconds(5);
                    options.BaseAddress = new Uri(builder.Configuration["DatabaseSettings:BackupServerUrl"]
                    ?? throw new Exception("Backup server base URL not found in configuration"));
                    DLogger.Log($"Backup server URL: {options.BaseAddress}");
                })
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    // Allow self-signed certificates for Android emulator (development only)
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });

                return builder.Build();
            }
            catch (Exception ex)
            {
                DLogger.Log("Error creating MauiApp", ex);
                throw;
            }

        }
    }
}