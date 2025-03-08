using System.Collections.Concurrent;
using System.Text.Json;
using BackupServer.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using BackupServer.Interfaces;

namespace BackupServer.Services
{

    public class BackupService : IBackupService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<BackupService> _logger;
        private readonly ConcurrentDictionary<int, SemaphoreSlim> _locks = new();
        private readonly JsonSerializerOptions _jsonOptions;

        public BackupService(IConfiguration configuration, IWebHostEnvironment env, ILogger<BackupService> logger)
        {
            _configuration = configuration;
            _env = env;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        // Helper method to get the backup folder path using configuration and the content root.
        private string GetBackupFolderPath()
        {
            var backupFolderFromConfig = _configuration["BackupSettings:BackupDirectory"] ?? "Users";
            var backupFolder = Path.Combine(_env.ContentRootPath, backupFolderFromConfig);
            if (!Directory.Exists(backupFolder))
            {
                Directory.CreateDirectory(backupFolder);
                _logger.LogInformation("Created backup folder at: {BackupFolder}", backupFolder);
            }
            return backupFolder;
        }

        public async Task<bool> PerformBackupAsync(int userId, List<BackupRequest> websites)
        {
            // Get or create a semaphore for the user.
            var semaphore = _locks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));
            _logger.LogInformation("Backup request received for user {UserId} using semaphore {SemaphoreHash}", userId, semaphore.GetHashCode());
            await semaphore.WaitAsync();
            try
            {
                var backupFolder = GetBackupFolderPath();
                var filePath = Path.Combine(backupFolder, $"{userId}.json");
                _logger.LogInformation("Backup file path: {FilePath}", filePath);

                var json = JsonSerializer.Serialize(websites, _jsonOptions);
                _logger.LogInformation("Serialized JSON: {JsonData}", json);

                await File.WriteAllTextAsync(filePath, json);
                _logger.LogInformation("Backup saved successfully for user {UserId}", userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing backup for user {UserId}", userId);
                return false;
            }
            finally
            {
                semaphore.Release();
                _logger.LogInformation("Backup request processing completed for user {UserId}", userId);
            }
        }

        public async Task<bool> HealthCheckAsync()
        {
            try
            {
                var backupFolder = GetBackupFolderPath();
                _logger.LogInformation("Performing health check on backup folder: {BackupFolder}", backupFolder);

                // Create a test JSON file
                var testFile = Path.Combine(backupFolder, "test.json");
                var testData = new BackupRequest
                {
                    UserId = -1,
                    WebsiteId = Guid.NewGuid(),
                    Name = "Test Website",
                    Url = "https://test.com",
                    DateCreatedAt = DateTime.UtcNow
                };

                var json = JsonSerializer.Serialize(testData);
                await File.WriteAllTextAsync(testFile, json);

                var readJson = await File.ReadAllTextAsync(testFile);
                var deserializedData = JsonSerializer.Deserialize<BackupRequest>(readJson);

                var isWritable = deserializedData?.Name == testData.Name;
                File.Delete(testFile);
                _logger.LogInformation("Health check completed successfully");
                return isWritable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Backup service health check failed");
                return false;
            }
        }
    }
}
