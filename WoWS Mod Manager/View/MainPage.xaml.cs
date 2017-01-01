using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Diagnostics;
using WoWS_Mod_Manager.Xaml;
using WoWS_Mod_Manager.Control;
using WoWS_Mod_Manager.ViewModel;
using WoWS_Mod_Manager.View;
using System.Threading.Tasks;

// Die Vorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 dokumentiert.

namespace WoWS_Mod_Manager
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            DataContext = App.instance.viewModel;
        }

        private async void DeployMods_Click(object sender, RoutedEventArgs e)
        {
            App.instance.viewModel.GlobalInterfaceAvailable = false;
            App.instance.viewModel.GlobalActionInProgress = true;
            await Task.Run(() => App.instance.modManager.Push());
            App.instance.viewModel.GlobalActionInProgress = false;
            App.instance.viewModel.GlobalInterfaceAvailable = true;
        }

        private void SaveMods_Click(object sender, RoutedEventArgs e)
        {
            App.instance.viewModel.GlobalInterfaceAvailable = false;
            App.instance.viewModel.GlobalActionInProgress = true;
            App.instance.storageManager.SaveSelectedModData(); //TODO asyncify
            App.instance.viewModel.GlobalActionInProgress = false;
            App.instance.viewModel.GlobalInterfaceAvailable = true;
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(Options));
        }
    }
}
