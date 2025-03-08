using WebKeepApp.Models;

namespace WebKeepApp.Interfaces
{
    public interface ILoginService
    {
        Task<User> GetSampleUserAsync();
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> IsLoggedInAsync();
        Task<bool> LoginAsync(string username, string password);
        Task<bool> RegisterAsync(string username, string password);
        Task<bool> LogoutAsync();
    }
}