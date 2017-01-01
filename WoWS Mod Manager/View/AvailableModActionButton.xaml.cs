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
using Windows.UI.Xaml.Navigation;
using WoWS_Mod_Manager.Control.Data;
using WoWS_Mod_Manager.ViewModel;
using WoWS_Mod_Manager.Xaml;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace WoWS_Mod_Manager.View
{
    public sealed partial class AvailableModActionButton : UserControl
    {
        public AvailableModActionButton()
        {
            this.InitializeComponent();
        }

        private async void AvailableModActionButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (!App.instance.viewModel.GlobalActionInProgress) /* ignore click if something is working */
            {
                Debug.WriteLine("AvailableModActionButton_ClickAsync " + DataContext);
                App.instance.viewModel.GlobalInterfaceAvailable = false;
                var button = sender as Button;
                var mod = DataContext as AvailableMods_ModViewModel;
                mod.Available = false;
                SelectedMods_ModViewModel selectedModel = new SelectedMods_ModViewModel(mod._Model);
                App.instance.viewModel.selectedMods.Add(selectedModel);
                selectedModel.ErrorTextVisibility = Visibility.Collapsed;
                ModDownloadProgress progress = new ModDownloadProgress(selectedModel);
                App.instance.viewModel.PendingActions.Add(progress);
                await Task.Run(async () =>
                {
                    JSONRootModHome home = await App.instance.modManager.FetchModHome(mod._Model);
                    if(home != null)
                    {
                        if(home.versions.Count > 0)
                        {
                            string latestVersion = home.versions[0].version;
                            await App.instance.modManager.HandleDownloadModActionAsync(mod._Model, progress, latestVersion);
                        } else
                        {
                            await mod._Model.selectedListViewModel.SetError("No version supplied");
                            await mod._Model.selectedListViewModel.SetImagePath("/Assets/Icons/update_fail_60.png");
                        }
                    }
                });
            }
        }
    }
}
