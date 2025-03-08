using WebKeepApp.Interfaces;
using WebKeepApp.Utils;

namespace WebKeepApp
{
    public partial class App : Application
    {
        private readonly IDatabaseService _databaseService;
        public App(IDatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;

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

        protected async override void OnStart()
        {
            DLogger.Log("OnStart");
            try
            {
                // Handle when your app starts
                await _databaseService.InitializeDatabaseAsync();
                DLogger.Log("Database initialization completed successfully in OnStart");
            }
            catch (Exception ex)
            {
                DLogger.Log($"Error during database initialization in OnStart: {ex.Message}");
                throw;
            }
        }
    }
}
