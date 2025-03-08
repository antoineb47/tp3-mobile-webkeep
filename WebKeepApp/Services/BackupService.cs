using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using WebKeepApp.Interfaces;
using WebKeepApp.Utils;

namespace WebKeepApp.Services
{
    public class BackupService : IBackupService
    {
        private readonly HttpClient _httpClient;
        private readonly IDatabaseService _databaseService;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _JsonOptions;

        public BackupService(IHttpClientFactory httpClient, IDatabaseService databaseService, IConfiguration configuration)
        {
            _httpClient = httpClient.CreateClient("BackupClient");
            _httpClient.Timeout = TimeSpan.FromSeconds(5);

            _databaseService = databaseService;
            _configuration = configuration;
            _JsonOptions = new()
            {
                WriteIndented = true
            };
        }
        public async Task<HttpResponseMessage> BackupUserDataAsync(int UserId)
        {
            try
            {
                var websitesList = await _databaseService.GetWebsitesForUserAsync(UserId);
                if (websitesList == null || websitesList.Count == 0)
                {
                    DLogger.Log($"No websites found for user {UserId}");
                    return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound)
                    {
                        Content = new StringContent("No websites found for user")
                    };
                }
                DLogger.Log($"Websites: {string.Join(", ", websitesList.Select(w => w.Url))}");

                var jsonContent = JsonConvert.SerializeObject(websitesList);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var responseBackup = await _httpClient.PostAsync($"{UserId}", httpContent);

                if (!responseBackup.IsSuccessStatusCode)
                {
                    DLogger.Log("Error backing up data");
                }
                else
                {
                    DLogger.Log($"Successfully backed up {websitesList.Count} websites for user {UserId}");
                }

                return responseBackup;
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error backing up database: {ex.Message}");
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent($"Error backing up database: {ex.Message}")
                };
            }
        }

        public async Task<HttpResponseMessage> HealthCheckAsync()
        {
            DLogger.Log($"Base Url for the request: {_httpClient.BaseAddress}");
            var responseHealth = await _httpClient.GetAsync("health");

            DLogger.Log($"Response status code: {responseHealth.StatusCode}");

            if (!responseHealth.IsSuccessStatusCode ||
            JsonConvert.DeserializeObject<dynamic>(await responseHealth.Content.ReadAsStringAsync())?.status != "Healthy")
            {
                DLogger.Log("Backup service is unhealthy");
            }
            else
            {
                DLogger.Log("Backup service is healthy");
            }

            return responseHealth;
        }
    }
}