using WebKeepApp.Models;

namespace WebKeepApp.Interfaces
{
    public interface IWebsiteService
    {
        Task<bool> WebsiteExistsForUserAsyncByUrl(string url, int userId);
        Task<bool> WebsiteExistsForUserByNameAsync(string name, int userId);
        Task<bool> CreateWebsiteAsync(string name, string url, int userId, string? note = null);
        Task<bool> UpdateWebsiteAsync(string websiteId, string name, string url, string? note = null);
        Task<bool> DeleteWebsiteAsync(string websiteId);
        Task<Website> GetWebsiteAsync(string websiteId);
        Task<IEnumerable<Website>> GetWebsitesForUserAsync(int userId);
        Task OpenWebsiteInChromeAsync(string url);
    }
}