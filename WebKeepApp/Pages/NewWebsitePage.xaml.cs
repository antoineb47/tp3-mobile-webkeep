using WebKeepApp.ViewModels;

namespace WebKeepApp.Pages;

public partial class NewWebsitePage : ContentPage
{
    public NewWebsitePage(NewWebsiteViewModel newWebsiteViewModel)
    {
        InitializeComponent();
        BindingContext = newWebsiteViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is NewWebsiteViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }
}
