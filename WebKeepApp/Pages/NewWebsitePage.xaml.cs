using WebKeepApp.Utils;
using WebKeepApp.ViewModels;

namespace WebKeepApp.Pages;

public partial class NewWebsitePage : ContentPage
{
    public NewWebsitePage(NewWebsiteViewModel newWebsiteViewModel)
    {
        InitializeComponent();
        BindingContext = newWebsiteViewModel;
        DLogger.Log("NewWebsitePage constructor called");
    }
}
