using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WebKeepApp.Interfaces;
using WebKeepApp.Models;
using WebKeepApp.Utils;

namespace WebKeepApp.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ILoginService _loginService;
        private readonly IWebsiteService _websiteService;

        [ObservableProperty]
        private string? username;

        [ObservableProperty]
        private string? searchText;

        [ObservableProperty]
        private ObservableCollection<Website> websites;

        private User user;

        public MainViewModel(ILoginService loginService, IWebsiteService websiteService)
        {
            _loginService = loginService;
            _websiteService = websiteService;

            DLogger.Log("MainViewModel created");
            _ = InitializeAsync();
        }

        public async Task InitializeAsync()
        {
            Websites = [];
            user = new User();

            await LoadUserAsync();
            await LoadWebsitesAsync();
        }

        private async Task LoadUserAsync()
        {
            try
            {
                user = await _loginService.GetLoggedUserAsync() ?? new User();
                DLogger.Log("User: " + user?.Username);
                Username = user?.Username ?? "Unknown";
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error loading user: {ex.Message}");
            }
        }

        private async Task LoadWebsitesAsync()
        {
            if (user?.Id == null) return;

            try
            {
                var websitesList = await _websiteService.GetWebsitesForUserAsync(user.Id);
                if (websitesList != null)
                {
                    Websites = [.. websitesList.Select(w => new Website
                    {
                        Name = w.Name,
                        Url = w.Url,
                        DateCreatedAt = w.DateCreatedAt
                    })];
                    DLogger.Log($"Loaded {websitesList.Count()} websites");
                }
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error loading websites: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task AddWebsiteAsync()
        {
            try
            {
                var user = await _loginService.GetLoggedUserAsync() ?? throw new InvalidOperationException("User is not logged in.");
                await Shell.Current.GoToAsync("AddWebsitePage");
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error navigating to AddWebsitePage: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task LogoutAsync()
        {
            try
            {
                DLogger.Log("Logging out...");
                // Handle logout
                var loggedOut = await _loginService.LogoutAsync(1);

                if (!loggedOut) return;

                // Clear user data
                Username = null;
                user = new User();
                Websites.Clear();
                await Shell.Current.GoToAsync("///LoginPage");
                DLogger.Log("User logged out");
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error during logout: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            try
            {
                DLogger.Log($"Searching for: {SearchText}");

                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    await LoadWebsitesAsync();
                }
                else
                {
                    var filtered = Websites
                    .Where(w =>
                    (!string.IsNullOrEmpty(w.Name) && w.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(w.Url) && w.Url.Contains(SearchText, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                    Websites = [.. filtered];
                }
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error during search: {ex.Message}");
            }
        }

    }
}