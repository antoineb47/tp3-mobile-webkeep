using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WebKeepApp.Interfaces;
using WebKeepApp.Models;
using WebKeepApp.Utils;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace WebKeepApp.ViewModels
{
    [QueryProperty(nameof(WebsiteId), "websiteId")]
    [QueryProperty(nameof(Mode), "mode")]
    public partial class NewWebsiteViewModel : ObservableObject
    {
        private readonly IWebsiteService _websiteService;
        private readonly ILoginService _loginService;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private string? name;

        [ObservableProperty]
        private string? url;

        [ObservableProperty]
        private string? note;

        [ObservableProperty]
        private bool isEditMode;

        [ObservableProperty]
        private string pageTitle;

        private string websiteId;

        public string WebsiteId
        {
            get => websiteId;
            set
            {
                websiteId = value;
                LoadWebsite(value);
            }
        }

        private string mode;
        public string Mode
        {
            get => mode;
            set
            {
                mode = value;
                IsEditMode = mode == "Edit";
                PageTitle = IsEditMode ? "Edit Website" : "New Website";
                ResetFields();
            }
        }

        public NewWebsiteViewModel(IWebsiteService websiteService, ILoginService loginService, IDialogService dialogService)
        {
            _websiteService = websiteService;
            _loginService = loginService;
            _dialogService = dialogService;
            pageTitle = string.Empty;
            websiteId = string.Empty;
            mode = string.Empty;
        }

        private async void LoadWebsite(string websiteId)
        {
            try
            {
                var website = await _websiteService.GetWebsiteAsync(websiteId);
                if (website != null)
                {
                    Name = website.Name;
                    Url = website.Url;
                    Note = website.Note;
                }
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error loading website: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                var user = await _loginService.GetLoggedUserAsync() ?? throw new InvalidOperationException("User is not logged in.");

                // Validate Name field
                if (string.IsNullOrEmpty(Name))
                {
                    await _dialogService.DisplayAlertAsync("Validation Error", "The Name field cannot be empty.", "OK");
                    return;
                }

                // Validate URL field
                if (!string.IsNullOrEmpty(Url))
                {
                    if (!Url.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ||
                        (!Url.EndsWith(".com", StringComparison.OrdinalIgnoreCase) && !Url.EndsWith(".ca", StringComparison.OrdinalIgnoreCase)))
                    {
                        await _dialogService.DisplayAlertAsync("Validation Error", "The URL must start with 'www.' and end with '.com' or '.ca'.", "OK");
                        return;
                    }
                }

                // Check if both Note and URL fields are empty
                if (string.IsNullOrEmpty(Note) && string.IsNullOrEmpty(Url))
                {
                    bool proceed = await _dialogService.DisplayConfirmAsync("Empty Fields", "Both Note and URL fields are empty. Do you want to proceed?", "Yes", "No");
                    if (!proceed)
                    {
                        return;
                    }
                }

                // Validate duplicate Name
                var existingWebsites = await _websiteService.GetWebsitesForUserAsync(user.Id);
                var existingWebsiteByName = existingWebsites?.FirstOrDefault(w => w.Name != null && w.Name.Equals(Name, StringComparison.OrdinalIgnoreCase));
                if (existingWebsiteByName != null && (!IsEditMode || (IsEditMode && existingWebsiteByName.Id != websiteId)))
                {
                    await _dialogService.DisplayAlertAsync("Validation Error", "A website with the same name already exists.", "OK");
                    return;
                }

                // Validate duplicate URL
                if (!string.IsNullOrEmpty(Url))
                {
                    var existingWebsiteByUrl = await _websiteService.WebsiteExistsForUserAsyncByUrl(Url, user.Id);
                    if (existingWebsiteByUrl)
                    {
                        bool proceed = await _dialogService.DisplayConfirmAsync("Duplicate URL", "A website with the same URL already exists. Do you want to proceed?", "Yes", "No");
                        if (!proceed)
                        {
                            return;
                        }
                    }
                }

                bool success;
                if (IsEditMode)
                {
                    success = await _websiteService.UpdateWebsiteAsync(websiteId, Name, Url ?? string.Empty, Note);
                }
                else
                {
                    success = await _websiteService.CreateWebsiteAsync(Name, Url ?? string.Empty, user.Id, Note);
                }

                if (success)
                {
                    DLogger.Log("Website saved successfully");
                    ResetFields();
                    await Shell.Current.GoToAsync("///MainPage");
                }
                else
                {
                    DLogger.Log("Failed to save website");
                }
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error saving website: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task DeleteAsync()
        {
            try
            {
                bool confirm = await _dialogService.DisplayConfirmAsync("Confirm Delete", "Are you sure you want to delete this website?", "Yes", "No");
                if (confirm)
                {
                    var success = await _websiteService.DeleteWebsiteAsync(websiteId);
                    if (success)
                    {
                        DLogger.Log("Website deleted successfully");
                        await _dialogService.DisplayAlertAsync("Success", "Website deleted successfully", "OK");
                        await Shell.Current.GoToAsync("///MainPage");
                    }
                    else
                    {
                        DLogger.Log("Failed to delete website");
                    }
                }
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error deleting website: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            DLogger.Log("Going back to main page");
            await Shell.Current.GoToAsync("///MainPage");
        }

        private void ResetFields()
        {
            Name = string.Empty;
            Url = string.Empty;
            Note = string.Empty;
        }
    }
}