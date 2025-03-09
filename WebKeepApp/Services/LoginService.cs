using WebKeepApp.Interfaces;
using WebKeepApp.Models;
using WebKeepApp.Utils;

namespace WebKeepApp.Services
{
    public class LoginService : ILoginService
    {
        private readonly IDatabaseService _databaseService;
        private readonly string _authTokenKey = "auth_token";

        public User? LoggedUser { get; private set; }

        public LoginService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<User> GetSampleUserAsync()
        {
            try
            {
                var listUsers = await _databaseService.GetUsersAsync();
                if (listUsers == null || listUsers.Count == 0)
                {
                    throw new InvalidOperationException("Sample user not found");
                }

                var random = new Random();
                int index = random.Next(listUsers.Count);
                var user = listUsers[index];

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
                var user = await GetUserByUsernameAsync(username);
                return user != null;
            }
            catch (Exception ex)
            {
                DLogger.Log($"An error occurred while checking if username exists: {ex.Message}");
                return false;
            }
        }

        public async Task<User?> GetLoggedUserAsync()
        {
            try
            {
                var token = await SecureStorage.GetAsync(_authTokenKey);
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

                var user = await GetUserByUsernameAndIdAsync(username, userId);
                if (user == null)
                {
                    DLogger.Log("User not found");
                    return null;
                }

                LoggedUser = user;
                DLogger.Log($"Logged user found: {user.Id}, {user.Username}");
                return user;
            }
            catch (Exception ex)
            {
                DLogger.Log($"An error occurred while getting logged user: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                var user = await GetUserByUsernameAndPasswordAsync(username, password);
                if (user == null)
                {
                    DLogger.Log("User with username " + username + " not found");
                    return false;
                }

                await SetLoggedInUserAsync(user);
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
                var userExists = await UsernameExistsAsync(username);
                if (userExists)
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

        public async Task<bool> LogoutAsync(int userId)
        {
            try
            {
                var loggedUser = await GetLoggedUserAsync();
                if (loggedUser == null || loggedUser.Id != userId)
                {
                    DLogger.Log("User not logged in or user ID mismatch");
                    return false;
                }

                await ClearLoggedInUserAsync();
                DLogger.Log("Auth token removed");
                return true;
            }
            catch (Exception ex)
            {
                DLogger.Log($"An error occurred during logout: {ex.Message}");
                return false;
            }
        }

        private async Task SetLoggedInUserAsync(User user)
        {
            LoggedUser = user;
            await SecureStorage.SetAsync(_authTokenKey, $"{user.Username}:{user.Id}");
        }

        private async Task ClearLoggedInUserAsync()
        {
            await Task.Run(() => SecureStorage.Remove(_authTokenKey));
            LoggedUser = null;
        }

        private async Task<User?> GetUserByUsernameAsync(string username)
        {
            var users = await _databaseService.GetUsersAsync();
            return users.FirstOrDefault(u =>
                u.Username != null && u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<User?> GetUserByUsernameAndIdAsync(string username, string userId)
        {
            var users = await _databaseService.GetUsersAsync();
            return users.FirstOrDefault(u =>
                u.Username == username && u.Id.ToString() == userId);
        }

        private async Task<User?> GetUserByUsernameAndPasswordAsync(string username, string password)
        {
            var users = await _databaseService.GetUsersAsync();
            return users.FirstOrDefault(u =>
                u.Username != null && u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                u.Password == password);
        }
    }
}