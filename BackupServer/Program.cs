using BackupServer.Services;
using BackupServer.Interfaces;
using BackupServer.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure JSON options
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.WriteIndented = true;
});

builder.Services.AddSingleton<IBackupService, BackupService>();

var app = builder.Build();

// Configure the HTTPs request pipeline for easier integration with Android emulator
app.UseHttpsRedirection();

// Mapping the endpoints using the BackupService
app.MapPost("/backup/{userId:int}", async (int userId, List<BackupRequest> websites, IBackupService backupService) =>
{
    var success = await backupService.PerformBackupAsync(userId, websites);
    return success ? Results.Ok(new { Message = "Backup saved successfully" })
                   : Results.Problem("Error processing backup");
});

app.MapGet("/backup/health", async (IBackupService backupService) =>
{
    var isHealthy = await backupService.HealthCheckAsync();
    return isHealthy ? Results.Ok(new { Status = "Healthy" })
                     : Results.Problem("Backup service unhealthy");
});

app.Run();
