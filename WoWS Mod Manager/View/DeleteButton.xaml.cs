using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WoWS_Mod_Manager.ViewModel;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace WoWS_Mod_Manager.View
{
    public sealed partial class DeleteButton : UserControl
    {
        public DeleteButton()
        {
            this.InitializeComponent();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!App.instance.viewModel.GlobalActionInProgress) /* ignore click if something is working */
            {
                SelectedMods_ModViewModel mod = DataContext as SelectedMods_ModViewModel;
                if(mod.Progress == null)
                {
                    Debug.WriteLine("DeleteButton_Click");
                    App.instance.viewModel.selectedMods.Remove(mod);
                    mod._Model.availableListViewModel.Available = true;
                }
            }
        }
    }
}
