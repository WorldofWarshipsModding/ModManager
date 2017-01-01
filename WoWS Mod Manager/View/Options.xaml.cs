using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WoWS_Mod_Manager.Control;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace WoWS_Mod_Manager.View
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class Options : Page
    {
        public Options()
        {
            this.InitializeComponent();
            DataContext = App.instance.viewModel;
            Debug.WriteLine("#########"+App.instance.viewModel.WoWSFolder);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(MainPage));
        }

        private async void SetWoWSLocationButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            SetWoWSLocation.IsEnabled = false;
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add(".exe"); /* api bug circumvention */
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace(Util.wowsFolderTest, folder); /* save permission */
                App.instance.viewModel.WoWSFolder = folder.Path;
                if (await Task.Run(() => App.instance.storageManager.SetNewWoWSFolder(folder)))
                {
                    OkButton.IsEnabled = true;
                }
            }
            SetWoWSLocation.IsEnabled = true;
        }
    }
}
