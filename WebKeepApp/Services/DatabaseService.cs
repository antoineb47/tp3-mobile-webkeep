using SQLite;
using WebKeepApp.Models;
using WebKeepApp.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using WebKeepApp.Utils;

namespace WebKeepApp.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly SQLiteAsyncConnection _database;

        public DatabaseService(IConfiguration configuration)
        {
            DLogger.Log("Database service constructor called");
            var dbFileName = configuration["DatabaseSettings:DatabaseFileName"] ?? throw new Exception("Database file name not found in configuration");
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, dbFileName);
            DLogger.Log($"Database file path: {dbPath}");
            _database = new SQLiteAsyncConnection(dbPath);
        }

        public async Task InitializeDatabase()
        {
            try
            {
                await _database.CreateTableAsync<User>();
                await _database.CreateTableAsync<Website>();
                DLogger.Log("Database initialized successfully");
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error initializing database: {ex.Message}");
                throw;
            }
        }

        public async Task AddUserAsync(User user)
        {
            try
            {
                await _database.InsertAsync(user);
                DLogger.Log("User added successfully");
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error adding user: {ex.Message}");
                throw;
            }
        }

        public async Task AddWebsiteAsync(Website website)
        {
            try
            {
                await _database.InsertAsync(website);
                DLogger.Log("Website added successfully");
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error adding website: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteUserAsync(User user)
        {
            try
            {
                await _database.DeleteAsync(user);
                DLogger.Log("User deleted successfully");
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error deleting user: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteWebsiteAsync(Website website)
        {
            try
            {
                await _database.DeleteAsync(website);
                DLogger.Log("Website deleted successfully");
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error deleting website: {ex.Message}");
                throw;
            }
        }

        public async Task<User> GetUserAsync(int userId)
        {
            try
            {
                var user = await _database.Table<User>().Where(u => u.Id == userId).FirstOrDefaultAsync();
                DLogger.Log("User retrieved successfully");
                return user;
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error retrieving user: {ex.Message}");
                throw;
            }
        }

        public async Task<List<User>> GetUsersAsync()
        {
            try
            {
                var users = await _database.Table<User>().ToListAsync();
                DLogger.Log("Users retrieved successfully");
                return users;
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error retrieving users: {ex.Message}");
                throw;
            }
        }

        public async Task<Website> GetWebsiteAsync(Guid websiteId)
        {
            try
            {
                var website = await _database.Table<Website>().Where(w => w.Guid == websiteId).FirstOrDefaultAsync();
                DLogger.Log("Website retrieved successfully");
                return website;
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error retrieving website: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Website>> GetWebsitesForUserAsync(int userId)
        {
            try
            {
                var websites = await _database.Table<Website>().Where(w => w.UserId == userId).ToListAsync();
                DLogger.Log("Websites retrieved successfully");
                return websites;
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error retrieving websites: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateUserAsync(User user)
        {
            try
            {
                await _database.UpdateAsync(user);
                DLogger.Log("User updated successfully");
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error updating user: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateWebsiteAsync(Website website)
        {
            try
            {
                await _database.UpdateAsync(website);
                DLogger.Log("Website updated successfully");
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error updating website: {ex.Message}");
                throw;
            }
        }
    }
}