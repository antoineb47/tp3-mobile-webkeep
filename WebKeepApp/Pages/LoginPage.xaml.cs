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
    private readonly IBackupService _backupService;

    public LoginPage(IDatabaseService databaseService, IBackupService backupService)
    {
        InitializeComponent();
        _databaseService = databaseService;
        _backupService = backupService;
        DLogger.Log("LoginPage constructor called");
    }

    private async void UsernameEntry_Focused(object sender, FocusEventArgs e)
    {
        try
        {
            var response = await _backupService.HealthCheckAsync();
            var response1 = await _backupService.BackupUserDataAsync(1);
            DLogger.Log($"LoginPage: Health check completed successfully: {response}");
            DLogger.Log($"LoginPage: Backup user data completed successfully: {response1}");
            DLogger.Log("Backup of user data completed successfully");
        }
        catch (Exception ex)
        {
            DLogger.Log($"Error adding user: {ex.Message}");
        }
    }
}
