using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WebKeepApp.Interfaces;
using WebKeepApp.Utils;
using System.Threading.Tasks;

namespace WebKeepApp.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? username;

        [ObservableProperty]
        private string? password;

        [ObservableProperty]
        private string? errorMessage;

        private readonly ILoginService _loginService;
        private readonly IDialogService _dialogService;

        private const string SuccessTitle = "Success";
        private const string AttentionTitle = "Attention";
        private const string InvalidCredentialsMessage = "Invalid username or password";
        private const string EnterCredentialsMessage = "Please enter username and password";
        private const string UserCreatedMessage = "User created successfully, you can now login";
        private const string CreateUserPromptMessage = "No user with this username, would you like to create one?";

        public LoginViewModel(ILoginService loginService, IDialogService dialogService)
        {
            _loginService = loginService;
            _dialogService = dialogService;

            username = string.Empty;
            password = string.Empty;
        }

        [RelayCommand]
        private async Task TestUserAsync()
        {
            try
            {
                var user = await _loginService.GetSampleUserAsync();
                DLogger.Log($"User retrieved successfully: {user.Username} and {user.Password}");

                Username = user.Username;
                Password = user.Password;
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error retrieving sample user: {ex.Message}");
                ErrorMessage = "Failed to retrieve sample user.";
            }
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = EnterCredentialsMessage;
                return;
            }

            try
            {
                var usernameExists = await _loginService.UsernameExistsAsync(Username);
                if (!usernameExists)
                {
                    bool createUser = await _dialogService.DisplayConfirmAsync(AttentionTitle, CreateUserPromptMessage, "Yes", "No");

                    if (createUser)
                    {
                        await CreateUserAsync();
                        return;
                    }

                    DLogger.Log("User chose not to create user");
                    ErrorMessage = "User creation was cancelled.";
                    return;
                }

                var logged = await _loginService.LoginAsync(Username, Password);

                if (!logged)
                {
                    ErrorMessage = InvalidCredentialsMessage;
                    DLogger.Log("Invalid username or password");
                    return;
                }

                DLogger.Log($"User logged successfully: {Username}");
                ErrorMessage = string.Empty;
                await Shell.Current.GoToAsync("///MainPage");
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error during login: {ex.Message}");
                ErrorMessage = "An error occurred during login.";
            }
        }

        private async Task CreateUserAsync()
        {
            try
            {
                if (Username != null && Password != null)
                {
                    await _loginService.RegisterAsync(Username, Password);
                    await ShowAlertAsync(SuccessTitle, UserCreatedMessage);
                    DLogger.Log($"User created successfully: {Username}");
                    Username = Password = string.Empty;
                    ErrorMessage = string.Empty;
                }
                else
                {
                    ErrorMessage = EnterCredentialsMessage;
                }
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error creating user: {ex.Message}");
                ErrorMessage = "An error occurred while creating the user.";
            }
        }

        private async Task ShowAlertAsync(string title, string message)
        {
            await _dialogService.DisplayAlertAsync(title, message, "OK");
        }
    }
}