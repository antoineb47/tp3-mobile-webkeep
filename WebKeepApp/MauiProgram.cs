﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebKeepApp.Interfaces;
using WebKeepApp.Services;
using WebKeepApp.Utils;
using WebKeepApp.Pages;
using WebKeepApp.ViewModels;

namespace WebKeepApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            try
            {
                var builder = MauiApp.CreateBuilder();
                DLogger.Log("Starting CreateMauiApp...");

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
                builder.Services.AddTransient<LoginViewModel>();
                builder.Services.AddTransient<MainPage>();
                builder.Services.AddTransient<MainViewModel>();
                builder.Services.AddTransient<CreatePage>();
                builder.Services.AddTransient<CreateViewModel>();
                DLogger.Log("Pages and views loaded successfully");

                // Register services
                builder.Services.AddSingleton<ILoginService, LoginService>();
                builder.Services.AddSingleton<IBackupService, BackupService>();
                builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
                builder.Services.AddSingleton<IDialogService, DialogService>();
                DLogger.Log("Services registered successfully");

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
                DLogger.Log("Backup client configured successfully");

                DLogger.Log("Building app...");
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