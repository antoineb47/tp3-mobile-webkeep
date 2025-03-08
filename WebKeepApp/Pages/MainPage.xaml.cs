using WebKeepApp.ViewModels;

namespace WebKeepApp.Pages;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel mainViewModel)
    {
        InitializeComponent();
        BindingContext = mainViewModel;
    }
}
