using WebKeepApp.Models;

namespace WebKeepApp.Interfaces
{
    public interface ILoginService
    {
        User? LoggedUser { get; }
        Task<User> GetSampleUserAsync();
        Task<bool> UsernameExistsAsync(string username);
        Task<User> GetLoggedUserAsync();
        Task<bool> LoginAsync(string username, string password);
        Task<bool> RegisterAsync(string username, string password);
        Task<bool> LogoutAsync(int userId);
    }
}