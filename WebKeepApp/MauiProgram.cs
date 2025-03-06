using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebKeepApp.Interfaces;
using WebKeepApp.Services;
using System.Threading.Tasks;
using WebKeepApp.Utils;

namespace WebKeepApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            try 
            {
                // Test debug message
        DLogger.Log("Testing simple debug message");

        // Test error message with exception
        try
        {
            throw new InvalidOperationException("This is a test error");
        }
        catch (Exception ex)
        {
            DLogger.Log("An error occurred during testing", ex);
        }
                DLogger.Log("Starting CreateMauiApp");
                var builder = MauiApp.CreateBuilder();
                DLogger.Log("Builder created");
        
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

                // Initialize database
                try 
                {
                    builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
                    var app = builder.Build();
                    var databaseService = app.Services.GetRequiredService<IDatabaseService>();
                    
                    DLogger.Log("Starting database initialization");
                    Task.Run(async () => {
                        try {
                            await databaseService.InitializeDatabase();
                            DLogger.Log("Database initialization completed successfully");
                        }
                        catch (Exception ex) {
                            DLogger.Log("Database initialization failed in background task", ex);
                        }
                    });
                    DLogger.Log("Returning app instance while database initializes");
                    return app;
                }
                catch (Exception ex)
                {
                    DLogger.Log("Failed to initialize database", ex);
                    throw;
                }
            } 
            catch (Exception ex) 
            {
                DLogger.Log("Fatal error in app initialization", ex);
                throw;
            }
        }
    }
}