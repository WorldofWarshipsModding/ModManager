using System;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Threading.Tasks;
using Windows.Storage;
using System.Text.RegularExpressions;
using Windows.Web.Http;
using System.Collections.Generic;
using WoWS_Mod_Manager.Control.Data;
using System.Collections.ObjectModel;
using WoWS_Mod_Manager.ViewModel;
using Newtonsoft.Json;
using WoWS_Mod_Manager.Xaml;

namespace WoWS_Mod_Manager.Control
{
    public class StorageManager
    {
        public const string TAG = "[StorageManager]";
        public const string wowsFolderSetting = "wows_folder";
        public const string wowsSelectedModsSetting = "wows_selectedmods";
        public Regex wowsVersionRegex = new Regex(@"^[0-9]*\.[0-9]*\.[0-9]*\.[0-9]*");
        public string versionNumber = null;

        public async Task CheckXmlRepository()
        {
            try
            {
                bool download = true;
                if (File.Exists(ApplicationData.Current.LocalFolder.Path + @"\v" + versionNumber + ".zip"))
                {
                    /* stale zip file, not looking good, cleaning up */
                    File.Delete(ApplicationData.Current.LocalFolder.Path + @"\v" + versionNumber + ".zip");
                    if (Directory.Exists(ApplicationData.Current.LocalFolder.Path + @"\v" + versionNumber))
                    {
                        Directory.Delete(ApplicationData.Current.LocalFolder.Path + @"\v" + versionNumber, true);
                    }
                }

                if (Directory.Exists(ApplicationData.Current.LocalFolder.Path + @"\v" + versionNumber))
                {
                    if (File.Exists(ApplicationData.Current.LocalFolder.Path + @"\v" + versionNumber + ".zip"))
                    {
                        Directory.Delete(ApplicationData.Current.LocalFolder.Path + @"\v" + versionNumber, true);
                        File.Delete(ApplicationData.Current.LocalFolder.Path + @"\v" + versionNumber + ".zip");
                    }
                    else
                    {
                        download = false;
                    }
                    /* folder only -> ok */
                }

                if (download)
                {
                    Debug.WriteLine("[StorageManager] downloading raw xmls for " + versionNumber);
                    HttpClient httpClient = new HttpClient();
                    HttpResponseMessage response = await httpClient.GetAsync(new Uri("https://github.com/WorldofWarshipsModding/wows_resources/archive/v" + versionNumber + ".zip"));
                    response.EnsureSuccessStatusCode();
                    StorageFile zipfile = await ApplicationData.Current.LocalFolder.CreateFileAsync("v" + versionNumber + ".zip");
                    Debug.WriteLine(zipfile.Path);
                    var stream = await zipfile.OpenAsync(FileAccessMode.ReadWrite);
                    var ostream = stream.GetOutputStreamAt(0);
                    await response.Content.WriteToStreamAsync(ostream);
                    ostream.Dispose();
                    stream.Dispose();
                    System.IO.Compression.ZipFile.ExtractToDirectory(ApplicationData.Current.LocalFolder.Path + @"\v" + versionNumber + ".zip", ApplicationData.Current.LocalFolder.Path + @"\v" + versionNumber);
                    File.Delete(ApplicationData.Current.LocalFolder.Path + @"\v" + versionNumber + ".zip");
                    //TODO do something if version not found
                }
            }
            catch (Exception e)
            {
                Debug.Write(e.Message);
                Debug.Write(e.StackTrace);
            }
        }

        internal bool SetNewWoWSFolder(StorageFolder folder)
        {
            if (!IsWoWSLocationValid())
            {
                return false;
            }
            if (!ReadWoWSVersion())
            {
                return false;
            }
            Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace(Util.wowsFolder, folder);
            ApplicationData.Current.LocalSettings.Values[Util.isMarkedOk] = true;
            ApplicationData.Current.LocalSettings.Values[Util.wowsFolder] = App.instance.viewModel.WoWSFolder;
            return true;
        }

        //after cleanup
        public bool IsMarkedOk()
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(Util.isMarkedOk) && (bool)ApplicationData.Current.LocalSettings.Values[Util.isMarkedOk])
                return true;
            return false;
        }

        private bool IsWoWSLocationValid()
        {
            try
            {
                String LauncherConfig = App.instance.viewModel.WoWSFolder + @"\WoWSLauncher.cfg";
                if (File.Exists(LauncherConfig))
                    return true;
                Debug.WriteLine("File.Exists returned false for " + LauncherConfig);
                return false;
            }
            catch (Exception e)
            {
                Debug.Write(e);
                return false;
            }
        }

        private bool ReadWoWSVersion()
        {
            try
            {
                string wowscfg = App.instance.viewModel.WoWSFolder + @"\WoWSLauncher.cfg";
                XmlDocument cfg = new XmlDocument();
                cfg.Load(File.OpenRead(wowscfg));
                XmlNode version = cfg.DocumentElement.GetElementsByTagName("client_ver").Item(0);
                Match match = wowsVersionRegex.Match(version.InnerText);
                if (match.Success)
                {
                    versionNumber = match.Value;
                }
                Debug.WriteLine("parsed version number " + versionNumber);
                return true;
            }
            catch (Exception e)
            {
                Debug.Write(e);
            }
            return false;
        }

        /* synchronos load on startup */
        public void Load()
        {
            string SavedMods = (string)ApplicationData.Current.LocalSettings.Values[wowsSelectedModsSetting];
            if (SavedMods != null)
            {
                try
                {
                    JSONRootModList modstorage = JsonConvert.DeserializeObject<JSONRootModList>(SavedMods);
                    foreach (Mod mod in modstorage.mods)
                    {
                        Debug.WriteLine(String.Format("{0} loaded mod {1} ({2})", TAG, mod.name, mod.localversion));
                        SelectedMods_ModViewModel selectedModel = new SelectedMods_ModViewModel(mod);
                        App.instance.viewModel.selectedMods.Add(new SelectedMods_ModViewModel(mod));

                        App.instance.viewModel.PendingActions.Add(mod);
                        Task.Run(async () => {
                            JSONRootModHome home = await App.instance.modManager.FetchModHome(mod);
                            if (home != null)
                            {
                                if (home.versions.Count > 0)
                                {
                                    //TODO compare versions
                                    await mod.selectedListViewModel.SetImagePath("/Assets/Icons/Icon_DL_New_60.png");
                                }
                            }
                            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                App.instance.viewModel.PendingActions.Remove(mod);
                                App.instance.viewModel.TryEnableGlobalInterface();
                            });
                        });
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    Debug.WriteLine(e.StackTrace);
                }
            }
            App.instance.viewModel.WoWSFolder = (string)ApplicationData.Current.LocalSettings.Values[Util.wowsFolder];
            ReadWoWSVersion();
        }

        public void SaveSelectedModData()
        {
            try
            {
                JSONRootModList storage = new JSONRootModList();
                foreach (SelectedMods_ModViewModel mod in App.instance.viewModel.selectedMods)
                {
                    storage.mods.Add(mod._Model);
                }
                ApplicationData.Current.LocalSettings.Values[wowsSelectedModsSetting] = JsonConvert.SerializeObject(storage);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }
    }
}
