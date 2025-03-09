using WebKeepApp.Interfaces;
using WebKeepApp.Models;
using WebKeepApp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace WebKeepApp.Services
{
    public class WebsiteService : IWebsiteService
    {
        private readonly IDatabaseService _databaseService;

        public WebsiteService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<bool> WebsiteExistsForUserAsyncByUrl(string url, int userId)
        {
            try
            {
                DLogger.Log($"Checking if website with URL: {url} exists for user with ID: {userId}");
                var websites = await _databaseService.GetWebsitesForUserAsync(userId);
                var exists = websites.Any(w => w.Url != null && w.Url.Equals(url, StringComparison.OrdinalIgnoreCase));
                DLogger.Log($"Website with URL: {url} {(exists ? "exists" : "does not exist")} for user with ID: {userId}");
                return exists;
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error checking if website with URL: {url} exists for user with ID: {userId}", ex);
                throw;
            }
        }

        public async Task<bool> WebsiteExistsForUserByNameAsync(string name, int userId)
        {
            try
            {
                DLogger.Log($"Checking if website with name: {name} exists for user with ID: {userId}");
                var websites = await _databaseService.GetWebsitesForUserAsync(userId);
                var exists = websites.Any(w => w.Name != null && w.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                DLogger.Log($"Website with name: {name} {(exists ? "exists" : "does not exist")} for user with ID: {userId}");
                return exists;
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error checking if website with name: {name} exists for user with ID: {userId}", ex);
                throw;
            }
        }

        public async Task<bool> CreateWebsiteAsync(string name, string url, int userId, string? note = null)
        {
            try
            {
                DLogger.Log($"Creating website with name: {name}, url: {url} for user with ID: {userId}");
                var newWebsite = new Website
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = name,
                    Url = url,
                    UserId = userId,
                    Note = note,
                    DateCreatedAt = DateTime.UtcNow
                };
                await _databaseService.AddWebsiteAsync(newWebsite);
                DLogger.Log($"Successfully created website with name: {name} for user with ID: {userId}");
                return true;
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error creating website with name: {name} for user with ID: {userId}", ex);
                throw;
            }
        }

        public async Task<bool> UpdateWebsiteAsync(string websiteId, string name, string url, string? note = null)
        {
            try
            {
                DLogger.Log($"Updating website with ID: {websiteId}");
                var website = await _databaseService.GetWebsiteAsync(websiteId);
                if (website == null)
                {
                    DLogger.Log($"Website with ID: {websiteId} not found");
                    return false;
                }

                website.Name = name;
                website.Url = url;
                website.Note = note;

                await _databaseService.UpdateWebsiteAsync(website);
                DLogger.Log($"Successfully updated website with ID: {websiteId}");
                return true;
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error updating website with ID: {websiteId}", ex);
                throw;
            }
        }

        public async Task<bool> DeleteWebsiteAsync(string websiteId)
        {
            try
            {
                DLogger.Log($"Deleting website with ID: {websiteId}");
                await _databaseService.DeleteWebsiteAsync(websiteId);
                DLogger.Log($"Successfully deleted website with ID: {websiteId}");
                return true;
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error deleting website with ID: {websiteId}", ex);
                throw;
            }
        }

        public async Task<Website> GetWebsiteAsync(string websiteId)
        {
            try
            {
                DLogger.Log($"Retrieving website with ID: {websiteId}");
                var website = await _databaseService.GetWebsiteAsync(websiteId);
                if (website == null)
                {
                    DLogger.Log($"Website with ID: {websiteId} not found");
                    throw new KeyNotFoundException($"Website with ID: {websiteId} not found");
                }
                DLogger.Log($"Successfully retrieved website with ID: {websiteId}");
                return website;
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error retrieving website with ID: {websiteId}", ex);
                throw;
            }
        }

        public async Task<IEnumerable<Website>> GetWebsitesForUserAsync(int userId)
        {
            try
            {
                DLogger.Log($"Retrieving websites for user with ID: {userId}");
                var websites = await _databaseService.GetWebsitesForUserAsync(userId);
                DLogger.Log($"Successfully retrieved {websites.Count} websites for user with ID: {userId}");
                return websites;
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error retrieving websites for user with ID: {userId}", ex);
                throw;
            }
        }

        public async Task OpenWebsiteInChromeAsync(string url)
        {
            try
            {
                await Browser.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
                DLogger.Log($"Successfully opened URL: {url} in the default browser.");
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error opening URL in browser: {ex.Message}");
            }
        }
    }
}
