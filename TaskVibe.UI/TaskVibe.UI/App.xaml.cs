using System.Windows;

namespace TaskVibe.UI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Auto-create database & tables if missing before showing MainWindow
                DatabaseConnectionFactory.EnsureDatabaseCreated();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Database Initialization Error: {ex.Message}",
                                "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}