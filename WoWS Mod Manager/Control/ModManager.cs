using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.Web.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage;
using WoWS_Mod_Manager.Control.Data;
using WoWS_Mod_Manager.ViewModel;
using System.Threading;
using Windows.Storage.Streams;
using WoWS_Mod_Manager.Xaml;
using Windows.UI.Xaml;

namespace WoWS_Mod_Manager.Control
{
    public class ModManager
    {
        String TmpFolder = ApplicationData.Current.LocalFolder.Path + @"\tmp\";
        String TargetFolder = ApplicationData.Current.LocalFolder.Path + @"\target\";
        String ModsFolder = ApplicationData.Current.LocalFolder.Path + @"\mods\";
        public ModManager()
        {

        }

        public void Push()
        {
            try
            {
                BuildResult result = new BuildResult();
                Merge(result).Wait();
                if (result.BuildSuccessful)
                {
                    Deploy();
                }
                //TODO feedback
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }

        }


        private async Task Merge(BuildResult buildResult)
        {
            if (Directory.Exists(TmpFolder)) Directory.Delete(TmpFolder, true);
            if (Directory.Exists(TargetFolder)) Directory.Delete(TargetFolder, true);
            Directory.CreateDirectory(TmpFolder);
            Directory.CreateDirectory(TargetFolder);
            await App.instance.storageManager.CheckXmlRepository();
            foreach (SelectedMods_ModViewModel modModel in App.instance.viewModel.selectedMods)
            {
                try
                {
                    Mod mod = modModel._Model;
                    String ModVersionFolder = ModsFolder + mod.identifier + @"\" + modModel.Version;
                    Debug.WriteLine("Merging Mod " + ModVersionFolder);

                    /* move files */
                    foreach (string file in Directory.EnumerateFiles(ModVersionFolder, "*.*", SearchOption.AllDirectories))
                    {
                        Debug.WriteLine(String.Format("handling file {0}", file));
                        string RelativeFile = file.Substring(ModVersionFolder.Length);
                        string OriginalFile = ApplicationData.Current.LocalFolder.Path + @"\v" + App.instance.storageManager.versionNumber + @"\wows_resources-" + App.instance.storageManager.versionNumber + @"\" + RelativeFile;
                        string TargetFile = TargetFolder + RelativeFile;
                        if (!File.Exists(OriginalFile))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(TargetFile));
                            if (!File.Exists(TargetFile))
                            {
                                Debug.WriteLine("[" + mod.identifier + "] copying " + RelativeFile);
                                File.Copy(file, TargetFile);
                            }
                            else
                            {
                                Debug.WriteLine("[" + mod.identifier + "] CANNOT MERGE " + RelativeFile);
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Handling " + RelativeFile);
                            if (RelativeFile == @"\gui\battle_elements.xml" || RelativeFile == @"gui\battle_elements.xml") /* battle_elements.xml has to be merged */
                            {
                                Debug.WriteLine("[" + mod.identifier + "] merging " + RelativeFile);
                                Directory.CreateDirectory(Path.GetDirectoryName(TargetFile));
                                if (!File.Exists(TargetFile))
                                {
                                    File.Copy(OriginalFile, TargetFile, false);
                                }

                                using (FileStream targetFile = File.Open(TargetFile, FileMode.Open))
                                using (FileStream srcFile = File.OpenRead(file))
                                {
                                    XmlDocument target = new XmlDocument();
                                    target.Load(targetFile);
                                    targetFile.Seek(0, SeekOrigin.Begin);
                                    XmlDocument src = new XmlDocument();
                                    src.Load(srcFile);

                                    XmlNode srcnode = src.DocumentElement;
                                    XmlNode dstnode = target.DocumentElement;
                                    MergeBattleElements(srcnode, dstnode);
                                    target.Save(targetFile);
                                }
                            }
                            else
                            {
                                if (!File.Exists(TargetFile))
                                {
                                    Debug.WriteLine("[" + mod.identifier + "] unable to merge, copying " + RelativeFile);
                                    File.Copy(file, ApplicationData.Current.LocalFolder.Path + @"\target\" + RelativeFile);
                                }
                                else
                                {
                                    Debug.WriteLine("[" + mod.identifier + "] unable to merge, is in confict " + RelativeFile);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("failed to merge " + modModel.Name);
                    Debug.WriteLine(e.Message);
                    Debug.WriteLine(e.StackTrace);
                }
            }
        }

        private String Fancify(XmlNode n)
        {
            string s = n.Name + " (";
            foreach (XmlAttribute attr in n.Attributes)
            {
                s += attr.Name + "=\"" + attr.Value + "\"";
            }
            s += ")";
            return s;
        }

        private bool MergeBattleElements(XmlNode src, XmlNode dst)
        {
            if (src.ChildNodes.Count > 0)
            {
                /* take care of child elements */
                foreach (XmlNode srcchild in src.ChildNodes)
                {
                    /* check if anything merge-worthy is there*/
                    XmlNode semiequal = null;
                    foreach (XmlNode dstchild in dst.ChildNodes)
                    {
                        if (IsSemiEqual(srcchild, dstchild))
                        {
                            semiequal = dstchild;
                            break;
                        }
                    }
                    if (semiequal != null)
                    {
                        //Debug.WriteLine(Fancify(srcchild) + " found " + Fancify(semiequal));
                        if (ShouldReplace(srcchild, semiequal))
                        {
                            dst.RemoveChild(semiequal);
                            dst.PrependChild(dst.OwnerDocument.ImportNode(srcchild, true));
                        }
                        else
                        {
                            MergeBattleElements(srcchild, semiequal);
                        }
                    }
                    else
                    {
                        //Debug.WriteLine(Fancify(srcchild) + " found no semiequal");
                        dst.PrependChild(dst.OwnerDocument.ImportNode(srcchild, true));
                    }
                }
            }
            return true;
        }


        /*
         * Two nodes are semiequal, iff:
         *  - they have the same name
         *  - they have the same clips or name attribute, or no attibutes
         */
        private bool IsSemiEqual(XmlNode src, XmlNode dst)
        {
            bool noattrs = true;
            if (src.Name != dst.Name)
                return false;

            foreach (XmlAttribute srcattr in src.Attributes)
            {
                noattrs = false;
                if (srcattr.Name == "clips" || srcattr.Name == "name")
                {
                    if (HasAndEquals(dst, srcattr))
                    {
                        return true;
                    }
                }
            }
            return noattrs;
        }

        private bool HasAndEquals(XmlNode n, XmlAttribute attr)
        {
            foreach (XmlAttribute dstattr in n.Attributes)
            {
                if (dstattr.Name == attr.Name && dstattr.Value == attr.Value)
                    return true;
            }
            return false;
        }

        private bool ShouldReplace(XmlNode src, XmlNode dst)
        {
            foreach (XmlAttribute srcattr in src.Attributes)
            {
                if (srcattr.Name == "name" && HasAndEquals(dst, srcattr))
                {
                    return true;
                }
                if (srcattr.Name == "clips" && HasAndEquals(dst, srcattr))
                {
                    return true;
                }
            }
            return false;
        }

        private bool Deploy()
        {
            try
            {
                string res_mods = App.instance.viewModel.WoWSFolder + @"\res_mods\";
                Debug.WriteLine("[ModManager] deploying target folder");
                foreach (string file in Directory.EnumerateFiles(TargetFolder, "*.*", SearchOption.AllDirectories))
                {
                    string RelativeFile = file.Substring(TargetFolder.Length);
                    string DeployPath = res_mods + @"\" + App.instance.storageManager.versionNumber + @"\" + RelativeFile;
                    Debug.WriteLine("[ModManager] deploying " + DeployPath);
                    Directory.CreateDirectory(Path.GetDirectoryName(DeployPath));
                    File.Copy(TargetFolder + RelativeFile, DeployPath, true);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
            return false;
        }

        /* checks home, "verifies" local version */
        public async Task<JSONRootModHome> FetchModHome(Mod mod)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "anything that is more than 5 characters because FUCK YOU GITHUB");
                HttpResponseMessage response = await httpClient.GetAsync(new Uri(mod.home));
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();
                JSONRootModHome result = JsonConvert.DeserializeObject<JSONRootModHome>(json);
                return result;
            } catch(Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
            return null;
        }

        public async Task HandleDownloadModActionAsync(Mod mod, ModDownloadProgress progress, String version)
        {
            try
            {
                await mod.selectedListViewModel.SetImagePath("/Assets/Icons/update_loading_60.png");
                progress.cancellationToken.Token.ThrowIfCancellationRequested();

                /* fetch .wowshome */
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "anything that is more than 5 characters because FUCK YOU GITHUB");
                HttpResponseMessage response = await httpClient.GetAsync(new Uri(mod.home)).AsTask(progress.cancellationToken.Token);
                progress.cancellationToken.Token.ThrowIfCancellationRequested();
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();
                JSONRootModHome result = JsonConvert.DeserializeObject<JSONRootModHome>(json);
                progress.cancellationToken.Token.ThrowIfCancellationRequested();

                ModRelease release = result.versions.ElementAt(0);
                await mod.selectedListViewModel.SetVersion(release.version);
                String ModVersionFolder = ModsFolder + mod.identifier + @"\" + release.version;
                if (Directory.Exists(ModVersionFolder)) Directory.Delete(ModVersionFolder, true);
                Directory.CreateDirectory(ModVersionFolder);

                /* download latest version zip */
                response = await httpClient.GetAsync(new Uri(release.archive)).AsTask(progress.cancellationToken.Token);
                response.EnsureSuccessStatusCode();
                progress.cancellationToken.Token.ThrowIfCancellationRequested();

                string zipfile = ModVersionFolder + mod.identifier + release.version + ".zip";
                IOutputStream zipstream = File.Create(zipfile).AsOutputStream();
                await response.Content.WriteToStreamAsync(zipstream);
                zipstream.Dispose();
                progress.cancellationToken.Token.ThrowIfCancellationRequested();

                /* extract zip */
                System.IO.Compression.ZipFile.ExtractToDirectory(ModVersionFolder + mod.identifier + release.version + ".zip", ModVersionFolder);
                File.Delete(zipfile);

                await mod.selectedListViewModel.SetImagePath("/Assets/Icons/deactivated_60.png");
                progress.success = true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
                if (Directory.Exists(ModsFolder + mod.identifier)) Directory.Delete(ModsFolder + mod.identifier + @"\" + version, true);
                await mod.selectedListViewModel.SetError(e.Message.Substring(0, e.Message.IndexOf(Environment.NewLine)));
                await mod.selectedListViewModel.SetImagePath("/Assets/Icons/update_fail_60.png");
            }
            finally
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    mod.selectedListViewModel.Progress = null;
                    App.instance.viewModel.PendingActions.Remove(progress);
                    App.instance.viewModel.TryEnableGlobalInterface();
                });
            }
        }
    }

    public class BuildResult
    {
        public bool BuildSuccessful = true;
        public bool DeploymentSuccessful = true;
        public List<ModResult> Failures = new List<ModResult>();
    }

    public class ModResult
    {

    }
}
