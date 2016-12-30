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

namespace WoWS_Mod_Manager.Control
{
    public class StorageManager
    {
        public ApplicationDataContainer storedSettings = ApplicationData.Current.LocalSettings;
        public StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        public ObservableCollection<SelectedMods_ModViewModel> selectedMods;
        public const string wowsFolderSetting = "wows_folder";
        public Regex wowsVersionRegex = new Regex(@"^[0-9]*\.[0-9]*\.[0-9]*\.[0-9]*");
        public string absoluteWoWSPath = null;
        public string versionNumber = null;
        public string availableVersion = null;
        private MainPage mainPage;
        public bool locationValid = false;
        public bool versionRead = false;
        public bool repositoryChecked = false;

        public StorageManager(MainPage mainPage)
        {
            this.mainPage = mainPage;
            selectedMods = mainPage.viewModel.selectedMods;
        }

        public async void Init()
        {
            await CheckWoWSLocation();
            await Task.Run(() => CheckWoWSVersion());
            await Task.Run(() => CheckXmlRepository());
            mainPage.UpdateGlobalInterfaceAvailable();
        }

        private async Task CheckWoWSLocation()
        {
            absoluteWoWSPath = (string)storedSettings.Values[wowsFolderSetting];
            if (absoluteWoWSPath == null || !await Task.Run(() => IsWoWSLocationValid(absoluteWoWSPath)))
            {
                var folderPicker = new Windows.Storage.Pickers.FolderPicker();
                folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
                folderPicker.FileTypeFilter.Add(".exe"); /* api bug circumvention */
                StorageFolder folder = await folderPicker.PickSingleFolderAsync();
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace(wowsFolderSetting, folder); /* save permission */
                if (folder != null && await Task.Run(() => IsWoWSLocationValid(folder.Path)))
                {
                    absoluteWoWSPath = folder.Path;
                    storedSettings.Values[wowsFolderSetting] = folder.Path;
                    Debug.WriteLine("Picked folder: " + folder.Name);
                    locationValid = true;
                    //TODO inform UI
                }
                else
                {
                    Debug.WriteLine("[StorageManager] Operation cancelled. " + folder?.Path);
                    //TODO inform UI
                }
            }
            else
            {
                locationValid = true;
            }
            Debug.WriteLine("using WoWS folder " + absoluteWoWSPath);
        }

        private bool IsWoWSLocationValid(string location)
        {
            Debug.WriteLine("IsWoWSLocationValid " + location);
            try
            {
                if (File.Exists(location + @"\WoWSLauncher.cfg"))
                    return true;
                Debug.WriteLine("no WoWSLauncher.cfg found at " + location);
                return false;
            }
            catch (Exception e)
            {
                Debug.Write(e);
                return false;
            }
        }

        private void CheckWoWSVersion()
        {
            try
            {
                string wowscfg = absoluteWoWSPath + @"\WoWSLauncher.cfg";
                XmlDocument cfg = new XmlDocument();
                cfg.Load(File.OpenRead(wowscfg));
                XmlNode version = cfg.DocumentElement.GetElementsByTagName("client_ver").Item(0);
                Match match = wowsVersionRegex.Match(version.InnerText);
                if (match.Success)
                {
                    versionNumber = match.Value;
                    versionRead = true;
                }
                Debug.WriteLine("parsed version number " + versionNumber);
            }
            catch (Exception e)
            {
                Debug.Write(e);
                //TODO inform UI
            }
        }

        private async Task CheckXmlRepository()
        {
            try
            {
                bool download = true;
                if (File.Exists(localFolder.Path + @"\v" + versionNumber + ".zip"))
                {
                    /* stale zip file, not looking good, cleaning up */
                    File.Delete(localFolder.Path + @"\v" + versionNumber + ".zip");
                    if (Directory.Exists(localFolder.Path + @"\v" + versionNumber))
                    {
                        Directory.Delete(localFolder.Path + @"\v" + versionNumber, true);
                    }
                }

                if (Directory.Exists(localFolder.Path + @"\v" + versionNumber))
                {
                    if (File.Exists(localFolder.Path + @"\v" + versionNumber + ".zip"))
                    {
                        Directory.Delete(localFolder.Path + @"\v" + versionNumber, true);
                        File.Delete(localFolder.Path + @"\v" + versionNumber + ".zip");
                    }
                    else
                    {
                        download = false;
                        repositoryChecked = true;
                    }
                    /* folder only -> ok */
                }

                if (download)
                {
                    Debug.WriteLine("[StorageManager] downloading raw xmls for " + versionNumber);
                    HttpClient httpClient = new HttpClient();
                    HttpResponseMessage response = await httpClient.GetAsync(new Uri("https://github.com/WorldofWarshipsModding/wows_resources/archive/v" + versionNumber + ".zip"));
                    response.EnsureSuccessStatusCode();
                    StorageFile zipfile = await localFolder.CreateFileAsync("v" + versionNumber + ".zip");
                    Debug.WriteLine(zipfile.Path);
                    var stream = await zipfile.OpenAsync(FileAccessMode.ReadWrite);
                    var ostream = stream.GetOutputStreamAt(0);
                    await response.Content.WriteToStreamAsync(ostream);
                    ostream.Dispose();
                    stream.Dispose();
                    System.IO.Compression.ZipFile.ExtractToDirectory(localFolder.Path + @"\v" + versionNumber + ".zip", localFolder.Path + @"\v" + versionNumber);
                    File.Delete(localFolder.Path + @"\v" + versionNumber + ".zip");
                    repositoryChecked = true;
                    //TODO do something if version not found
                }
            }
            catch (Exception e)
            {
                Debug.Write(e);
            }

        }
    }
}
