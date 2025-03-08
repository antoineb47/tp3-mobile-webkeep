using WebKeepApp.Utils;
using WebKeepApp.ViewModels;

namespace WebKeepApp.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel loginViewModel)
    {
        InitializeComponent();
        BindingContext = loginViewModel;
        DLogger.Log("LoginPage constructor called");
    }
}
