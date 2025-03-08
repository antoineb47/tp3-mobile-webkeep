using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WebKeepApp.Interfaces;
using WebKeepApp.Utils;

namespace WebKeepApp.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? username;

        [ObservableProperty]
        private string? password;
        private readonly ILoginService _loginService;
        private readonly IDialogService _dialogService;

        public LoginViewModel(ILoginService loginService, IDialogService dialogService)
        {
            _loginService = loginService;
            _dialogService = dialogService;
        }

        [RelayCommand]
        private async Task TestUserAsync()
        {
            var user = await _loginService.GetSampleUserAsync();
            DLogger.Log($"User retrieved successfully: {user.Username} and {user.Password}");

            Username = user.Username;
            Password = user.Password;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                await _dialogService.DisplayAlertAsync("Error", "Please enter username and password", "OK");
                return;
            }

            var usernameExists = await _loginService.UsernameExistsAsync(Username);
            if (!usernameExists)
            {
                bool createUser = await _dialogService.DisplayConfirmAsync
                    ("Attention", "No user with this username, would you like to create one?", "Yes", "No");

                if (createUser)
                {
                    // Redirect to user creation logic
                    await _loginService.RegisterAsync(Username, Password);
                    await _dialogService.DisplayAlertAsync("Success", "User created successfully, you can now login", "OK");
                    DLogger.Log($"User created successfully: {Username} and {Password}");
                    return;
                }

                DLogger.Log("User chose to not create user");
                return;
            }

            await _loginService.LoginAsync(Username, Password);
            DLogger.Log($"User logged successfully : {Username} and {Password}");
        }
    }
}