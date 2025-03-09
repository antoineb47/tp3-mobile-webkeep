using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WebKeepApp.Interfaces;
using WebKeepApp.Models;
using WebKeepApp.Utils;

namespace WebKeepApp.ViewModels
{
    [QueryProperty(nameof(WebsiteId), "websiteId")]
    [QueryProperty(nameof(Mode), "mode")]
    public partial class NewWebsiteViewModel : ObservableObject
    {
        public string? Mode { get; set; }
        public string? WebsiteId { get; set; }

        [ObservableProperty]
        private bool isEditMode;

        [ObservableProperty]
        private string? name;

        [ObservableProperty]
        private string? url;

        [ObservableProperty]
        private string? note;

        [ObservableProperty]
        private string pageTitle = "Default Title";
        private Website? website;

        private readonly IWebsiteService _websiteService;
        private readonly ILoginService _loginService;
        private readonly IDialogService _dialogService;


        public NewWebsiteViewModel(IWebsiteService websiteService, ILoginService loginService, IDialogService dialogService)
        {
            _websiteService = websiteService;
            _loginService = loginService;
            _dialogService = dialogService;

            _ = InitializeAsync();
        }

        public async Task InitializeAsync()
        {
            IsEditMode = Mode == "Edit";
            PageTitle = IsEditMode ? "Edit website" : "Create website";
            try
            {
                if (IsEditMode)
                {
                    DLogger.Log($"Initializing in edit mode with websiteId: {WebsiteId}");
                    if (WebsiteId != null)
                    {
                        await LoadWebsiteAsync(WebsiteId);
                    }
                    else
                    {
                        DLogger.Log("Error: WebsiteId is null in edit mode.");
                        ResetFields();
                    }
                }
                else
                {
                    DLogger.Log("Initializing in create mode");
                    ResetFields();
                }
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error initializing: {ex.Message}");
            }
        }

        private async Task LoadWebsiteAsync(string websiteId)
        {
            try
            {
                website = await _websiteService.GetWebsiteAsync(websiteId);
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

                if (string.IsNullOrEmpty(Name))
                {
                    await _dialogService.DisplayAlertAsync("Validation Error", "The Name field cannot be empty.", "OK");
                    return;
                }

                if (!string.IsNullOrEmpty(Url))
                {
                    if (!Url.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ||
                        (!Url.EndsWith(".com", StringComparison.OrdinalIgnoreCase) && !Url.EndsWith(".ca", StringComparison.OrdinalIgnoreCase)))
                    {
                        await _dialogService.DisplayAlertAsync("Validation Error", "The URL must start with 'www.' and end with '.com' or '.ca'.", "OK");
                        return;
                    }
                }

                if (string.IsNullOrEmpty(Note) && string.IsNullOrEmpty(Url))
                {
                    bool proceed = await _dialogService.DisplayConfirmAsync("Empty Fields", "Both Note and URL fields are empty. Do you want to proceed?", "Yes", "No");
                    if (!proceed)
                    {
                        return;
                    }
                }

                var existingWebsites = await _websiteService.GetWebsitesForUserAsync(user.Id);
                var existingWebsiteByName = existingWebsites?.FirstOrDefault(w => w.Name != null && w.Name.Equals(Name, StringComparison.OrdinalIgnoreCase));

                if (existingWebsiteByName != null && !IsEditMode && existingWebsiteByName.Id != WebsiteId)
                {
                    await _dialogService.DisplayAlertAsync("Validation Error", "A website with the same name already exists.", "OK");
                    return;
                }

                // Validate duplicate URL
                if (!string.IsNullOrEmpty(Url))
                {
                    bool duplicateUrlExists = await _websiteService.WebsiteExistsForUserAsyncByUrl(Url, user.Id);
                    if (duplicateUrlExists && !IsEditMode)
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
                    if (WebsiteId == null)
                    {
                        await _dialogService.DisplayAlertAsync("Error", "Website ID is missing. Unable to update website.", "OK");
                        return;
                    }
                    success = await _websiteService.UpdateWebsiteAsync(WebsiteId, Name, Url ?? string.Empty, Note);
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
                    if (WebsiteId == null)
                    {
                        await _dialogService.DisplayAlertAsync("Error", "Website ID is not set.", "OK");
                        return;
                    }
                    var success = await _websiteService.DeleteWebsiteAsync(WebsiteId);
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
            try
            {
                DLogger.Log("Cancelling");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error cancelling: {ex.Message}");
            }
        }

        private void ResetFields()
        {
            Name = string.Empty;
            Url = string.Empty;
            Note = string.Empty;
        }
    }
}