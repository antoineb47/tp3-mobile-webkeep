namespace WebKeepApp.Interfaces
{
    public interface IBackupService
    {
        Task<HttpResponseMessage> BackupUserDataAsync(int userId);
        Task<HttpResponseMessage> HealthCheckAsync();
    }
}