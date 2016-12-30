﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using WoWS_Mod_Manager.Control.Data;

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

        override public String ToString()
        {
            return Name;
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
            }
        }
    }

    public class SelectedMods_ModViewModel : ModViewModel
    {
        public SelectedMods_ModViewModel(Mod mod) : base(mod)
        {
            _Model.selectedListViewModel = this;
        }
    }
}