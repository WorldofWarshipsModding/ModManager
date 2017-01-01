using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Windows.Web.Http;
using Windows.Storage;
using System.Diagnostics;
using System.IO;
using WoWS_Mod_Manager.Control.Data;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using WoWS_Mod_Manager.ViewModel;

namespace WoWS_Mod_Manager.Control
{
    public class HiveManager
    {
        public string archiveUrl = @"https://raw.githubusercontent.com/WorldofWarshipsModding/wows_modarchive/master/wows.modarchive";

        public HiveManager()
        {

        }

        /* lazy hive load, seperate task */
        public async Task FetchMods()
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "any words that is more than 5 characters");
                HttpResponseMessage response = await httpClient.GetAsync(new Uri(archiveUrl));
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();
                JSONRootModList result = JsonConvert.DeserializeObject<JSONRootModList>(json);
                result.mods.ForEach(mod =>
                {
                    AvailableMods_ModViewModel mv;
                    Mod sel = App.instance.viewModel.TryGetSelected(mod);
                    if (sel != null)
                    {
                        mv = new AvailableMods_ModViewModel(sel);
                        mv.Available = false;
                    }
                    else
                    {
                        mv = new AvailableMods_ModViewModel(mod);
                    }
                    App.instance.viewModel.availableMods.Add(mv);
                });
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }

        }
    }
}
