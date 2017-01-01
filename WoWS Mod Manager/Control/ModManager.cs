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

namespace WoWS_Mod_Manager.Control
{
    public class ModManager
    {
        string TmpFolder = ApplicationData.Current.LocalFolder.Path + @"\tmp\";
        string TargetFolder = ApplicationData.Current.LocalFolder.Path + @"\target\";
        private MainPage mainPage;
        public ModManager(MainPage mainPage)
        {
            this.mainPage = mainPage;
        }

        public async Task Build()
        {
            try
            {
                await Merge();
                await Task.Run(()=>Deploy());
            }
            catch (Exception e)
            {
                Debug.Write(e.StackTrace);
            }
        }

        public async Task Merge()
        {
            //http://stackoverflow.com/questions/14435520/why-use-httpclient-for-synchronous-connection DEADLOCK
            await ApplicationData.Current.LocalFolder.CreateFolderAsync("tmp", CreationCollisionOption.ReplaceExisting);
            await ApplicationData.Current.LocalFolder.CreateFolderAsync("target", CreationCollisionOption.ReplaceExisting);

            foreach (SelectedMods_ModViewModel modModel in mainPage.viewModel.selectedMods)
            {
                try
                {
                    Mod mod = modModel._Model;
                    /* fetch .wowshome */
                    HttpClient httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "anything that is more than 5 characters because FUCK YOU GITHUB");
                    HttpResponseMessage response = await httpClient.GetAsync(new Uri(mod.home));
                    response.EnsureSuccessStatusCode();
                    string json = await response.Content.ReadAsStringAsync();
                    JSONRootModHome result = JsonConvert.DeserializeObject<JSONRootModHome>(json);

                    /* update ModData */
                    modModel.LatestVersion = result.versions.ElementAt(0).version;

                    /* download latest version zip */
                    response = await httpClient.GetAsync(new Uri(result.versions.ElementAt(0).archive));
                    response.EnsureSuccessStatusCode();

                    /* extract zip */
                    StorageFile zipfile = await mainPage.modStorage.localFolder.CreateFileAsync(@"\tmp\" + mod.identifier + ".zip", CreationCollisionOption.ReplaceExisting);
                    using (var stream = await zipfile.OpenAsync(FileAccessMode.ReadWrite))
                    using (var ostream = stream.GetOutputStreamAt(0))
                    {
                        await response.Content.WriteToStreamAsync(ostream);

                    }
                    string ModTmpFolder = (TmpFolder + mod.identifier + @"\");
                    System.IO.Compression.ZipFile.ExtractToDirectory(mainPage.modStorage.localFolder.Path + @"\tmp\" + mod.identifier + ".zip", ModTmpFolder);
                    File.Delete(zipfile.Path);

                    /* move files */
                    foreach (string file in Directory.EnumerateFiles(ModTmpFolder, "*.*", SearchOption.AllDirectories))
                    {
                        string RelativeFile = file.Substring(ModTmpFolder.Length);
                        string OriginalFile = mainPage.modStorage.localFolder.Path + @"\v" + mainPage.modStorage.versionNumber + @"\wows_resources-" + mainPage.modStorage.versionNumber + @"\" + RelativeFile;
                        string TargetFile = mainPage.modStorage.localFolder.Path + @"\target\" + RelativeFile;
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
                            if (RelativeFile == @"gui\battle_elements.xml") /* battle_elements.xml has to be merged */
                            {
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
                                    File.Copy(file, mainPage.modStorage.localFolder.Path + @"\target\" + RelativeFile);
                                }
                                else
                                {
                                    Debug.WriteLine("[" + mod.identifier + "] CANNOT MERGE " + RelativeFile);
                                }
                            }
                        }
                    }
                } catch(Exception e)
                {
                    Debug.WriteLine("failed to merge "+ modModel.Name);
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
                        Debug.WriteLine(Fancify(srcchild) + " found " + Fancify(semiequal));
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
                        Debug.WriteLine(Fancify(srcchild) + " found no semiequal");
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

        private void Deploy()
        {
            string res_mods = mainPage.modStorage.absoluteWoWSPath + @"\res_mods\";
            foreach (string file in Directory.EnumerateFiles(TargetFolder, "*.*", SearchOption.AllDirectories))
            {
                string RelativeFile = file.Substring(TargetFolder.Length);
                string DeployPath = res_mods + @"\" + mainPage.modStorage.versionNumber + @"\" + RelativeFile;
                Directory.CreateDirectory(Path.GetDirectoryName(DeployPath));
                Debug.WriteLine(String.Format("{0} {1}", TargetFolder+RelativeFile, DeployPath));
                File.Copy(TargetFolder + RelativeFile, DeployPath, true);
            }
        }
    }
}
