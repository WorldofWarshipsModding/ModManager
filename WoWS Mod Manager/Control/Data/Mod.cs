using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWS_Mod_Manager.ViewModel;

namespace WoWS_Mod_Manager.Control.Data
{
    public class Mod
    {
        public AvailableMods_ModViewModel availableListViewModel;
        public SelectedMods_ModViewModel selectedListViewModel;


        public string identifier { get; set; }
        public string name { get; set; }
        public string license { get; set; }
        public string description { get; set; }
        public string home { get; set; }
        public bool eligible { get; set; } = false;

        override public string ToString()
        {
            return name + " ("+identifier+")";
        }
    }

    public class ModData
    {
        public string image { get; set; }
        public string archive { get; set; }
    }

    public class JSONRootModList
    {
        public List<Mod> mods { get; set; }
    }
    public class JSONRootModHome
    {
        public List<ModData> versions { get; set; }
    }
}
