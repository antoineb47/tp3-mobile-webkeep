using WebKeepApp.Interfaces;
using WebKeepApp.Models;
using WebKeepApp.Utils;

namespace WebKeepApp.Services
{
    public class LoginService : ILoginService
    {
        private readonly IDatabaseService _databaseService;
        public LoginService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        private async Task<bool> UserExistsAsync(string username, string password)
        {
            try
            {
                var users = await _databaseService.GetUsersAsync();
                var user = users.FirstOrDefault(u =>
                u.Username != null && u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                u.Password == password);

                if (user == null)
                {
                    DLogger.Log("User with username " + username + " not found");
                    return false;
                }

                DLogger.Log("User with username " + username + " found");
                return user != null;
            }
            catch (Exception ex)
            {
                DLogger.Log($"An error occurred while checking if user exists: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                var exists = await UserExistsAsync(username, password);

                if (!exists)
                {
                    DLogger.Log("User with username " + username + " not found");
                    return false;
                }

                DLogger.Log($"User logged successfully: {username}");
                return true;
            }
            catch (Exception ex)
            {
                DLogger.Log($"An error occurred during login: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RegisterAsync(string username, string password)
        {
            try
            {
                var exists = await UserExistsAsync(username, password);

                if (exists)
                {
                    DLogger.Log("User with username " + username + " already exists");
                    return false;
                }

                var user = new User
                {
                    Username = username,
                    Password = password
                };

                await _databaseService.AddUserAsync(user);
                DLogger.Log("User with username " + username + " registered successfully");
                return true;
            }
            catch (Exception ex)
            {
                DLogger.Log($"An error occurred during registration: {ex.Message}");
                return false;
            }
        }

        public Task<bool> LogoutAsync()
        {
            // Your logout logic here
            return Task.FromResult(true);
        }

        public async Task<User> GetSampleUserAsync()
        {
            var listUsers = await _databaseService.GetUsersAsync();
            var user = listUsers.FirstOrDefault() ?? throw new InvalidOperationException("Sample user not found");
            DLogger.Log($"User found: {user.Id}, {user.Username}, {user.Password}");
            return user;
        }

        public Task<bool> IsLoggedInAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            var users = await _databaseService.GetUsersAsync();
            var user = users.FirstOrDefault(u =>
            u.Username != null && u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                DLogger.Log("Username not found");
                return false;
            }

            DLogger.Log($"Username found: {user?.Id}, {user?.Username}, {user?.Password}");
            return true;
        }
    }
}