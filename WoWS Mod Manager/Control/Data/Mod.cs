using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWS_Mod_Manager.ViewModel;

namespace WoWS_Mod_Manager.Control.Data
{
    public class Mod
    {
        [JsonIgnore]
        public Boolean readlocal;
        [JsonIgnore]
        public AvailableMods_ModViewModel availableListViewModel;
        [JsonIgnore]
        public SelectedMods_ModViewModel selectedListViewModel;

        public bool ShouldSerializeLocalVersion()
        {
            Debug.WriteLine("ShouldSerializelocalVersion");
            return readlocal;
        }
        public string LocalVersion { get; set; }


        public string identifier { get; set; }
        public string name { get; set; }
        public string license { get; set; }
        public string description { get; set; }
        public string home { get; set; }
        public string category { get; set; } = "None";
        public string website { get; set; }
        public string author { get; set; }
        public string screenshot { get; set; }
        override public string ToString()
        {
            return name + " ("+identifier+")";
        }
    }

    public class ModData
    {
        public String version { get; set; }
        public String minWoWSVersion { get; set; }
        public string archive { get; set; }
    }

    public class JSONRootModList
    {
        public List<Mod> mods { get; set; } = new List<Mod>();
    }
    public class JSONRootModHome
    {
        public string screenshot { get; set; }
        public List<ModData> versions { get; set; }
    }
}
