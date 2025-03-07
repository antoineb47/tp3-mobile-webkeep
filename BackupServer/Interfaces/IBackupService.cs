using BackupServer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackupServer.Interfaces
{
    public interface IBackupService
    {
        Task<bool> PerformBackupAsync(int userId, List<BackupRequest> websites);
        Task<bool> HealthCheckAsync();
    }
}
