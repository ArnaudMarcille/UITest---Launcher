using MahApps.Metro.Controls;
using UITest___Launcher.ViewModels;

namespace UITest___Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            AndroidForm.DataContext = new AppFormViewModel();
            IOSForm.DataContext = new AppFormViewModel();
        }
    }
}
