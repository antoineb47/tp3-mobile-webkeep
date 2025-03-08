using WebKeepApp.Interfaces;
using WebKeepApp.Utils;

namespace WebKeepApp.Services
{
    public class DialogService : IDialogService
    {
        public async Task DisplayAlertAsync(string title, string message, string cancel)
        {
            try
            {
                await Shell.Current.DisplayAlert(title, message, cancel);
                DLogger.Log("Alert displayed successfully");
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error displaying alert: {ex.Message}");
                throw;
            }

        }

        public async Task<bool> DisplayConfirmAsync(string title, string message, string accept, string cancel)
        {
            try
            {
                var result = await Shell.Current.DisplayAlert(title, message, accept, cancel);
                DLogger.Log("Confirmation displayed successfully");
                return result;
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error displaying confirmation: {ex.Message}");
                throw;
            }
        }
    }
}