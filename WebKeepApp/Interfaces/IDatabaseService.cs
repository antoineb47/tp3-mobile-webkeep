using System.Collections.Generic;
using System.Threading.Tasks;
using WebKeepApp.Models;
using System;

namespace WebKeepApp.Interfaces
{
    public interface IDatabaseService
    {
        Task InitializeDatabaseAsync();
        Task AddUserAsync(User user);
        Task AddWebsiteAsync(Website website);
        Task DeleteUserAsync(User user);
        Task DeleteWebsiteAsync(Website website);
        Task<User> GetUserAsync(int userId);
        Task<List<User>> GetUsersAsync();
        Task<Website> GetWebsiteAsync(string websiteId);
        Task<List<Website>> GetWebsitesForUserAsync(int userId);
        Task UpdateUserAsync(User user);
        Task UpdateWebsiteAsync(Website website);
    }
}