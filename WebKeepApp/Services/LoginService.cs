using WebKeepApp.Interfaces;
using WebKeepApp.Models;
using WebKeepApp.Utils;

namespace WebKeepApp.Services
{
    public class LoginService : ILoginService
    {
        private readonly IDatabaseService _databaseService;
        private readonly string _authToken = "auth_token";
        public LoginService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        private async Task<User?> UserExistsAsync(string username, string password)
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
                    return null;
                }

                DLogger.Log("User with username " + username + " found");
                return user;
            }
            catch (Exception ex)
            {
                DLogger.Log($"An error occurred while checking if user exists: {ex.Message}");
                return null;
            }
        }

        private async Task<bool> IsLoggedInAsync()
        {
            var authToken = await SecureStorage.GetAsync(_authToken);
            return !string.IsNullOrEmpty(authToken);
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                var user = await UserExistsAsync(username, password);

                if (user == null)
                {
                    DLogger.Log("User with username " + username + " not found");
                    return false;
                }

                DLogger.Log($"User logged successfully: {username}");
                await SecureStorage.SetAsync(_authToken, new string($"{user.Username}:{user.Id}"));
                DLogger.Log($"Token: {SecureStorage.GetAsync(_authToken)}");
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
                var user = await UserExistsAsync(username, password);

                if (user != null)
                {
                    DLogger.Log("User with username " + username + " already exists");
                    return false;
                }

                var newUser = new User
                {
                    Username = username,
                    Password = password
                };

                await _databaseService.AddUserAsync(newUser);
                DLogger.Log("User with username " + username + " registered successfully");
                return true;
            }
            catch (Exception ex)
            {
                DLogger.Log($"An error occurred during registration: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> LogoutAsync()
        {
            var logged = await IsLoggedInAsync();

            if (!logged)
            {
                DLogger.Log("User not logged in");
                return false;
            }

            try
            {
                SecureStorage.Remove(_authToken);
                DLogger.Log("User logged out successfully");
                return true;
            }
            catch (Exception ex)
            {
                DLogger.Log($"An error occurred during logout: {ex.Message}");
                return false;
            }
        }

        public async Task<User> GetSampleUserAsync()
        {
            try
            {
                var listUsers = await _databaseService.GetUsersAsync();
                var user = listUsers.FirstOrDefault() ?? throw new InvalidOperationException("Sample user not found");
                DLogger.Log($"User found: {user.Id}, {user.Username}, {user.Password}");
                return user;
            }
            catch (Exception ex)
            {
                DLogger.Log($"An error occurred while getting sample user: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            try
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
            catch (Exception ex)
            {
                DLogger.Log($"An error occurred while checking if username exists: {ex.Message}");
                return false;
            }
        }

        public async Task<User> GetLoggedUserAsync()
        {
            try
            {
                var token = await SecureStorage.GetAsync(_authToken);
                if (string.IsNullOrEmpty(token))
                {
                    DLogger.Log("No token found");
                    return null;
                }

                var parts = token.Split(':');
                if (parts.Length != 2)
                {
                    DLogger.Log("Invalid token format");
                    return null;
                }

                var username = parts[0];
                var userId = parts[1];

                var users = await _databaseService.GetUsersAsync();
                var user = users.FirstOrDefault(u => u.Username == username && u.Id.ToString() == userId);

                if (user == null)
                {
                    DLogger.Log("User not found");
                    return null;
                }

                DLogger.Log($"Logged user found: {user.Id}, {user.Username}");
                return user;
            }
            catch (Exception ex)
            {
                DLogger.Log($"An error occurred while getting logged user: {ex.Message}");
                return null;
            }
        }
    }
}