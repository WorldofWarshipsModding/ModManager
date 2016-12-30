using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WoWS_Mod_Manager.Control.Data;
using WoWS_Mod_Manager.ViewModel;

namespace WoWS_Mod_Manager.Xaml
{
    public class GlobalViewModel : INotifyPropertyChanged
    {
        protected internal void OnPropertyChanged(string propertyname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<SelectedMods_ModViewModel> selectedMods { get; set; } = new ObservableCollection<SelectedMods_ModViewModel>();
        public ObservableCollection<AvailableMods_ModViewModel> availableMods { get; set; } = new ObservableCollection<AvailableMods_ModViewModel>();

        private bool _GlobalInterfaceAvailable = false;
        public bool GlobalInterfaceAvailable
        {
            get { return _GlobalInterfaceAvailable; }
            set
            {
                if (_GlobalInterfaceAvailable == value) return;
                _GlobalInterfaceAvailable = value;
                OnPropertyChanged("GlobalInterfaceAvailable");
            }
        }
    }
}
