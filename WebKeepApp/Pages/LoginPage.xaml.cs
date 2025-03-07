using System.Diagnostics;
using Microsoft.Maui.Controls;
using WebKeepApp.Interfaces;
using WebKeepApp.Models;
using WebKeepApp.Services;
using WebKeepApp.Utils;

namespace WebKeepApp.Pages;

public partial class LoginPage : ContentPage
{
    private readonly IDatabaseService _databaseService;
    
    public LoginPage(IDatabaseService databaseService)
    {
        InitializeComponent();
        _databaseService = databaseService;
        DLogger.Log("LoginPage constructor called");
    }

    private async void UsernameEntry_Focused(object sender, FocusEventArgs e)
    {
        try
        {
            // Create user with required fields using constructor
            var user = new User("TestUser1", "TestPassword1");
            var website = new Website(user.Id, "testwebsite", "wwww.testwebsite.com", "this is a note");
            
            // Use async/await when calling database operations
            await _databaseService.AddUserAsync(user);
            await _databaseService.AddWebsiteAsync(website);
            
            DLogger.Log($"User {user.Username} added successfully");
        }
        catch (Exception ex)
        {
            DLogger.Log($"Error adding user: {ex.Message}");
        }
    }
}
