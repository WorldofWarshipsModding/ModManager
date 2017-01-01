using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using WoWS_Mod_Manager.Control.Data;
using WoWS_Mod_Manager.Xaml;

namespace WoWS_Mod_Manager.ViewModel
{
    public class ModViewModel : INotifyPropertyChanged
    {
        public Mod _Model;
        public event PropertyChangedEventHandler PropertyChanged;

        public ModViewModel(Mod mod)
        {
            _Model = mod;
            
        }
        protected internal void OnPropertyChanged(string propertyname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }

        public string Name
        {
            get { return _Model.name; }
            set
            {
                if (_Model.name == value) return;
                _Model.name = value;
                OnPropertyChanged("Name");
            }
        }

        public string Category
        {
            get { return _Model.category; }
            set
            {
                if (_Model.category == value) return;
                _Model.category = value;
                OnPropertyChanged("Category");
            }
        }

        public string Website
        {
            get { return _Model.website; }
            set
            {
                if (_Model.website == value) return;
                _Model.website = value;
                OnPropertyChanged("Website");
            }
        }

        public string Author
        {
            get { return _Model.author; }
            set
            {
                if (_Model.author == value) return;
                _Model.author = value;
                OnPropertyChanged("Author");
            }
        }

        public string Screenshot
        {
            get { return _Model.screenshot; }
            set
            {
                if (_Model.screenshot == value) return;
                _Model.screenshot = value;
                OnPropertyChanged("Screenshot");
            }
        }

        override public String ToString()
        {
            return base.ToString() + " " +Name;
        }
    }

    public class AvailableMods_ModViewModel : ModViewModel
    {
        public AvailableMods_ModViewModel(Mod mod): base(mod)
        {
            _Model.availableListViewModel = this;
        }

        private bool _Available = true;
        public bool Available
        {
            get { return _Available; }
            set
            {
                if (_Available == value) return;
                _Available = value;
                OnPropertyChanged("Available");
                OnPropertyChanged("ImagePath");
            }
        }

        public string ImagePath
        {
            get {
                if (_Available)
                    return "/Assets/Icons/Icon_DL_New_60.png";
                return "/Assets/Icons/deactivated_60.png";
            }
        }
    }

    public class SelectedMods_ModViewModel : ModViewModel
    {
        public ModDownloadProgress Progress { get; set; }
        public SelectedMods_ModViewModel(Mod mod) : base(mod)
        {
            _Model.selectedListViewModel = this;
        }


        public async Task SetImagePath(string path)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                ImagePath = path;
            });
        }

        public async Task SetVersion(string version)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Debug.WriteLine("Setting " + this + " to " + version);
                Version = version;
            });
        }

        public async Task SetError(string error)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                ErrorTextVisibility = Visibility.Visible;
                ErrorText = error;
            });
        }




        public string Version
        {
            get { return _Model.localversion; }
            set
            {
                if (_Model.localversion == value) return;
                _Model.localversion = value;
                OnPropertyChanged("Version");
            }
        }

        private string _ImagePath = "/Assets/Icons/update_loading_60.png";
        public string ImagePath
        {
            get { return _ImagePath; }
            set
            {
                if (_ImagePath == value) return;
                _ImagePath = value;
                OnPropertyChanged("ImagePath"); 
            }
        }

        private Visibility _ErrorTextVisibility = Visibility.Collapsed;
        public Visibility ErrorTextVisibility
        {
            get { return _ErrorTextVisibility; }
            set
            {
                if (_ErrorTextVisibility == value) return;
                _ErrorTextVisibility = value;
                OnPropertyChanged("ErrorTextVisibility");
            }
        }

        private string _ErrorText = "";
        public string ErrorText
        {
            get { return _ErrorText; }
            set
            {
                if (_ErrorText == value) return;
                _ErrorText = value;
                OnPropertyChanged("ErrorText");
            }
        }
    }
}
