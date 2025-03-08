using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WebKeepApp.Interfaces;
using WebKeepApp.Models;

namespace WebKeepApp.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ILoginService _loginService;
        private readonly IDatabaseService _databaseService;

        [ObservableProperty]
        private string? username;

        [ObservableProperty]
        private string? searchText;

        [ObservableProperty]
        private ObservableCollection<Website> websites;

        private User? user;

        public MainViewModel(ILoginService loginService, IDatabaseService databaseService)
        {
            _loginService = loginService;
            _databaseService = databaseService;

            websites = new ObservableCollection<Website>();

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await LoadUserAsync();
            await LoadWebsitesAsync();
        }

        private async Task LoadUserAsync()
        {
            user = await _loginService.GetLoggedUserAsync();
            Username = user?.Username ?? "Unknown";
        }

        private async Task LoadWebsitesAsync()
        {
            if (user == null)
            {
                // Handle the case where the user is not logged in
                return;
            }

            var websitesList = await _databaseService.GetWebsitesForUserAsync(user.Id);
            Websites = new ObservableCollection<Website>(websitesList);
        }

        [RelayCommand]
        private async void Button1()
        {
            // Handle Button 1 click - Add a new website
            if (user == null)
            {
                // Handle the case where the user is not logged in
                return;
            }

            var newWebsite = new Website
            {
                Name = "New Website",
                Url = "http://newwebsite.com",
                UserId = user.Id
            };

            await _databaseService.AddWebsiteAsync(newWebsite);
            Websites.Add(newWebsite);
        }

        [RelayCommand]
        private async void Button2()
        {
            // Handle logout
            await _loginService.LogoutAsync();
            // Clear user data
            Username = null;
            user = null;
            Websites.Clear();
        }

        [RelayCommand]
        private void Search()
        {
            // Filter websites based on SearchText
            var filteredWebsites = (Websites ?? new ObservableCollection<Website>())
                .Where(website => !string.IsNullOrEmpty(SearchText)
                && !string.IsNullOrEmpty(website.Name) && website.Name.Contains
                (SearchText, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Websites = new ObservableCollection<Website>(filteredWebsites);
        }
    }
}