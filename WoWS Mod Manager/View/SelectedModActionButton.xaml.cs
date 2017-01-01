using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using WoWS_Mod_Manager.ViewModel;
using WoWS_Mod_Manager.Xaml;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace WoWS_Mod_Manager.View
{
    public sealed partial class SelectedModActionButton : UserControl
    {
        public SelectedModActionButton()
        {
            this.InitializeComponent();
        }

        private async void SelectedModActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (!App.instance.viewModel.GlobalActionInProgress) /* ignore click if something is working */
            {
                Debug.WriteLine("SelectedModActionButton_Click " + DataContext);
                SelectedMods_ModViewModel mod = DataContext as SelectedMods_ModViewModel;

                if(mod.ImagePath == "/Assets/Icons/update_fail_60.png" && mod.Progress == null)
                {
                    App.instance.viewModel.GlobalInterfaceAvailable = false;
                    mod.ErrorTextVisibility = Visibility.Collapsed;
                    ModDownloadProgress progress = new ModDownloadProgress(mod);
                    App.instance.viewModel.PendingActions.Add(progress);
                    
                    //await Task.Run(() => App.instance.modManager.HandleDownloadModActionAsync(mod._Model, progress));
                    mod.Progress = null;
                    App.instance.viewModel.PendingActions.Remove(progress);
                    App.instance.viewModel.TryEnableGlobalInterface();
                }
            }
        }
    }
}
