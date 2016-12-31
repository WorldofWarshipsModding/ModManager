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
        public StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        public ObservableCollection<AvailableMods_ModViewModel> availableMods;
        public MainPage mainPage;

        public HiveManager(MainPage mp)
        {
            mainPage = mp;
            availableMods = mp.viewModel.availableMods;
        }
        public async void Init()
        {
            await FetchMods();
        }

        public async Task FetchMods()
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "any words that is more than 5 characters");
            HttpResponseMessage response = await httpClient.GetAsync(new Uri(archiveUrl));
            response.EnsureSuccessStatusCode();
            if (File.Exists(localFolder.Path + @"\wowsmods.archive"))
            {
                File.Delete(localFolder.Path + @"\wowsmods.archive");
            }
            StorageFile wowsarchive = await localFolder.CreateFileAsync("wowsmods.archive");
            Debug.WriteLine(wowsarchive.Path);
            var stream = await wowsarchive.OpenAsync(FileAccessMode.ReadWrite);
            var ostream = stream.GetOutputStreamAt(0);
            string json = await response.Content.ReadAsStringAsync();
            JSONRootModList result = JsonConvert.DeserializeObject<JSONRootModList>(json);
            result.mods.ForEach(mod =>
            {
                AvailableMods_ModViewModel mv;
                Mod sel = mainPage.viewModel.TryGetSelected(mod);
                if(sel!=null)
                {
                    mv = new AvailableMods_ModViewModel(sel);
                    mv.Available = false;
                } else
                {
                    mv = new AvailableMods_ModViewModel(mod);
                }
                availableMods.Add(mv);
            });
        }
    }
}
