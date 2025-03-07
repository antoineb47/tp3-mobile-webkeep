using SQLite;
using WebKeepApp.Models;
using WebKeepApp.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using WebKeepApp.Utils;
using Bogus;

namespace WebKeepApp.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly IConfiguration _configuration;
        private readonly SQLiteAsyncConnection _database;
        private readonly TableMapping[] _tables = [new(typeof(User)), new(typeof(Website))];

        public DatabaseService(IConfiguration configuration)
        {
            _configuration = configuration;

            DLogger.Log("Database service constructor called");
            var dbFileName = configuration["DatabaseSettings:DatabaseFileName"] ?? throw new Exception("Database file name not found in configuration");
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, dbFileName);
            DLogger.Log($"Database file path: {dbPath}");
            
            _database = new SQLiteAsyncConnection(dbPath,
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex,
            storeDateTimeAsTicks : false);
            DLogger.Log("Database connection created");
        }

        private async Task ResetDatabaseAsync(TableMapping[] tables)
        {
            try
            {
                foreach (var table in tables)
                {
                    await _database.DropTableAsync(table);
                    DLogger.Log($"Dropped table: {table.TableName}");
                }
                await _database.CreateTablesAsync<User, Website>();
                DLogger.Log("Tables recreated successfully");
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error dropping tables: {ex.Message}");
                throw;
            }
        }

        private async Task SeedDatabaseAsync(int numberOfUsers)
        {
            try
            {
                await ResetDatabaseAsync(_tables);
                DLogger.Log("Database reset successfully");

                Random random = new();

                // Generate and insert users first
                var fakeUsers = new Faker<User>()
                    .CustomInstantiator(f => new User(
                        username: f.Internet.UserName(),
                        password: f.Internet.Password()
                    ))
                    .Generate(numberOfUsers);

                // Insert users and get their assigned IDs
                await _database.InsertAllAsync(fakeUsers);
                
                // Query back the inserted users to get their auto-generated IDs
                var insertedUsers = await _database.Table<User>().ToListAsync();
                DLogger.Log($"Added {insertedUsers.Count} fake users");
                DLogger.Log($"Users: {string.Join(", ", insertedUsers.Select(u => u.Username))}");

                // Generate websites for each user with their correct IDs
                foreach (var user in insertedUsers)
                {
                    var websites = new Faker<Website>()
                        .CustomInstantiator(f => new Website(
                            userId: user.Id,
                            name: f.Internet.DomainName(),
                            url: f.Internet.Url(),
                            notes: f.Lorem.Sentence()
                        ))
                        .Generate(random.Next(1, 3));

                    await _database.InsertAllAsync(websites);
                    DLogger.Log($"Added {websites.Count} fake websites for user {user.Id}");
                    DLogger.Log($"Websites: {string.Join(", ", websites.Select(w => w.Url))}");
                }

                DLogger.Log($"Database seeded with {numberOfUsers} users and their websites");
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error seeding database: {ex.Message}");
                throw;
            }
        }

        public async Task InitializeDatabaseAsync()
        {
            try
            {
                var reseed = _configuration.GetValue<bool>("DatabaseSettings:ReseedData");

                await _database.CreateTablesAsync<User, Website>();
                DLogger.Log("Database initialized successfully");

                if (reseed)
                    await SeedDatabaseAsync(5);
                    DLogger.Log("Database seeded successfully");
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

        public async Task<Website> GetWebsiteAsync(string websiteId)
        {
            try
            {
                var website = await _database.Table<Website>().Where(w => w.Id == websiteId).FirstOrDefaultAsync();
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