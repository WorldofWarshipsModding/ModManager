using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Diagnostics;
using WoWS_Mod_Manager.Xaml;
using WoWS_Mod_Manager.Control;
using WoWS_Mod_Manager.ViewModel;

// Die Vorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 dokumentiert.

namespace WoWS_Mod_Manager
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage instance;
        public StorageManager modStorage;
        public HiveManager hiveManager;
        public GlobalViewModel viewModel;
        public ModManager modManager;
        public MainPage()
        {
            instance = this;
            this.InitializeComponent();
            viewModel = new GlobalViewModel();
            DataContext = viewModel;

            modStorage = new StorageManager(this);
            hiveManager = new HiveManager(this);
            modManager = new ModManager(this);

            modStorage.Init();
            hiveManager.Init();
        }

        public void UpdateGlobalInterfaceAvailable()
        {
            viewModel.GlobalInterfaceAvailable = modStorage.locationValid & modStorage.versionRead & modStorage.repositoryChecked;
        }

        public void ModInstall_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var mod = button.Tag as AvailableMods_ModViewModel;
            Debug.WriteLine("adding mod " + mod);
            mod.Available = false;
            viewModel.selectedMods.Add(new SelectedMods_ModViewModel(mod._Model));
        }

        public void ModUninstall_Click(object sender, RoutedEventArgs e)
        {

            var button = sender as Button;
            var mod = button.Tag as SelectedMods_ModViewModel;
            Debug.WriteLine("removing mod " + mod);
            viewModel.selectedMods.Remove(mod);
            mod._Model.availableListViewModel.Available = true;
        }

        private void DeployMods_Click(object sender, RoutedEventArgs e)
        {
            viewModel.GlobalInterfaceAvailable = false;
            Deploy();
        }

        private async void Deploy()
        {
            await modManager.Build();
            viewModel.GlobalInterfaceAvailable = true;
        }

        private void RefreshMods_Click(object sender, RoutedEventArgs e)
        {
            modStorage.SaveSelectedModData();
        }
    }
}
