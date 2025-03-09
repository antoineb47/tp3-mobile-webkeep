using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WebKeepApp.Interfaces;
using WebKeepApp.Models;
using WebKeepApp.Utils;
using System.Linq;
using System.Threading.Tasks;

namespace WebKeepApp.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? username;

        [ObservableProperty]
        private string? searchText;

        [ObservableProperty]
        private ObservableCollection<Website> websites;

        private readonly ILoginService _loginService;
        private readonly IWebsiteService _websiteService;
        private readonly IDialogService _dialogService;

        private readonly IBackupService _backupService;

        private List<Website> allWebsites;
        private User user;

        public MainViewModel(ILoginService loginService, IWebsiteService websiteService, IBackupService backupService, IDialogService dialogService)
        {
            _loginService = loginService;
            _websiteService = websiteService;
            _backupService = backupService;
            _dialogService = dialogService;

            Websites = new ObservableCollection<Website>();
            allWebsites = new List<Website>();
            user = new User();

            DLogger.Log("MainViewModel created");
            _ = InitializeAsync();
        }

        public async Task InitializeAsync()
        {
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
                    allWebsites = websitesList.ToList();
                    FilterWebsites();
                    DLogger.Log($"Loaded {websitesList.Count()} websites");
                }
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error loading websites: {ex.Message}");
            }
        }

        private void FilterWebsites()
        {
            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? allWebsites
                : allWebsites.Where(w =>
                    (!string.IsNullOrEmpty(w.Name) && w.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(w.Url) && w.Url.Contains(SearchText, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            Websites.Clear();
            foreach (var website in filtered)
            {
                Websites.Add(website);
            }
        }

        [RelayCommand]
        private async Task AddWebsiteAsync()
        {
            try
            {
                var user = await _loginService.GetLoggedUserAsync() ?? throw new InvalidOperationException("User is not logged in.");
                await Shell.Current.GoToAsync("NewWebsitePage?mode=Create");
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error navigating to NewWebsitePage: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task LogoutAsync()
        {
            try
            {
                bool confirm = await _dialogService.DisplayConfirmAsync("Logout", "Are you sure you want to logout?", "Yes", "No");
                if (!confirm) return;

                DLogger.Log("Logging out...");
                var loggedOut = await _loginService.LogoutAsync(user.Id);

                if (!loggedOut) return;

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
        private void Search()
        {
            try
            {
                DLogger.Log($"Searching for: {SearchText}");
                FilterWebsites();
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error during search: {ex.Message}");
            }
        }
        [RelayCommand]
        private async Task BackupAsync()
        {
            try
            {
                bool confirm = await _dialogService.DisplayConfirmAsync("Backup", "Do you want to backup your data?", "Yes", "No");
                if (!confirm) return;

                // Perform the backup service healthcheck first
                DLogger.Log("Performing backup service healthcheck...");
                var healthResponse = await _backupService.HealthCheckAsync();
                if (!healthResponse.IsSuccessStatusCode)
                {
                    DLogger.Log("Backup service health check failed.");
                    await _dialogService.DisplayAlertAsync("Backup", "Backup service is currently unavailable. Please try again later.", "OK");
                    return;
                }

                DLogger.Log("Backing up data...");
                await _backupService.BackupUserDataAsync(user.Id);
                DLogger.Log("Data backed up successfully");

                await _dialogService.DisplayAlertAsync("Backup", "Data have been backed up successfully.", "OK");
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error during backup: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task ShowOptionsAsync(Website website)
        {
            try
            {
                var result = await _dialogService.DisplayConfirmAsync("Options", "Choose an option", "Go to Website", "Edit Website");

                if (result)
                {
                    DLogger.Log($"Opening URL in Chrome: {website.Url}");
                    if (!string.IsNullOrEmpty(website.Url))
                    {
                        await _websiteService.OpenWebsiteInChromeAsync(website.Url);
                    }
                    else
                    {
                        DLogger.Log("Website URL is null or empty.");
                    }
                }
                else
                {
                    DLogger.Log($"Navigating to NewWebsitePage for editing: {website.Id}");
                    await Shell.Current.GoToAsync($"NewWebsitePage?websiteId={website.Id}&mode=Edit");
                }
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error showing options: {ex.Message}");
            }
        }
    }
}