using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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

        public bool GlobalActionInProgress { get; set; } = true;

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

        public Mod TryGetSelected(Mod m)
        {
            foreach (SelectedMods_ModViewModel selected in selectedMods)
            {
                if (selected._Model.identifier == m.identifier)
                {
                    return selected._Model;
                }
            }
            return null;
        }

        public void TryEnableGlobalInterface()
        {
            //Debug.WriteLine("TryEnableGlobalInterface");
            if(PendingActions.Count == 0)
                GlobalInterfaceAvailable = true;
        }

        //after cleanup
        public const string TAG = "[ViewModel]";
        private string _WoWSFolder;
        public String WoWSFolder
        {
            get
            {
                return _WoWSFolder;
            }
            set
            {
                if (_WoWSFolder == value) return;
                _WoWSFolder = value;
                Debug.WriteLine(String.Format("{0} setting folder to {1}", TAG, value));
                OnPropertyChanged("WoWSFolder");
            }
        }

        public List<object> PendingActions = new List<object>();
    }

    public class ModDownloadProgress
    {
        public CancellationTokenSource cancellationToken = new CancellationTokenSource();
        public SelectedMods_ModViewModel Mod;
        public bool success = false;
        
        public ModDownloadProgress(SelectedMods_ModViewModel mod)
        {
            Mod = mod;
            Mod.Progress = this;
        }
    }

    public class ModHomeResult
    {

    }
}
