using System.Windows;

namespace Querty.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var mainWindow = new MainWindow() { DataContext = new MainViewModel() };
            mainWindow.Show();
            base.OnStartup(e);
        }
    }
}
