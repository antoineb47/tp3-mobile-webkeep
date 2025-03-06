using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace WebKeepApp.pages;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private void UsernameEntry_Focused(object sender, FocusEventArgs e)
    {
        Debug.WriteLine("MyDebugTag: Username entry focused.");
    }
}
