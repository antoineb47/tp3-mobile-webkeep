using WebKeepApp.Utils;

namespace WebKeepApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            try
            {
                MainPage = new AppShell();
                DLogger.Log("MainPage set successfully in App constructor");
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error setting MainPage in App constructor: {ex.Message}");
                throw;
            }
        }
    }
}
