using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SQLiteNetExtensions.Attributes;
using WebKeepApp.Interfaces;

namespace WebKeepApp.ViewModels
{
    public partial class NewWebsiteViewModel : ObservableObject
    {
        private readonly IWebsiteService _websiteService;

        public NewWebsiteViewModel(IWebsiteService websiteService)
        {
            _websiteService = websiteService;
        }
        [RelayCommand]
        private async Task AddWebsiteAsync()
        {


        }
    }
}