using WebKeepApp.Utils;
using WebKeepApp.ViewModels;

namespace WebKeepApp.Pages;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel mainViewModel)
    {
        InitializeComponent();
        BindingContext = mainViewModel;
        DLogger.Log("MainPage constructor called");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        DLogger.Log("MainPage OnAppearing called");
        // Add code here to refresh data or update UI.
        if (BindingContext is MainViewModel vm)
        {
            Task.Run(() => vm.InitializeAsync());
        }
    }
}
